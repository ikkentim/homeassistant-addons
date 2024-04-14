using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SolisScraper.Models;

namespace SolisScraper;

public class ScraperInstance
{
    private readonly MqttTransmitter _mqttClient;
    private readonly string _discoveryPrefix;
    private readonly InstanceConfiguration _options;
    private readonly ILogger _logger;
    private State _state = State.Initial;
    private readonly SolarClient _solarClient;
    private SolarScrapeResult _previousResult;
    private int _failures;
    private int _format;
    private bool _sleepResultSent;

    public ScraperInstance(MqttTransmitter mqttClient, string discoveryPrefix, InstanceConfiguration options, ILogger logger)
    {
        _mqttClient = mqttClient;
        _discoveryPrefix = discoveryPrefix;
        _options = options;
        _logger = logger;

        _format = _options.Format;
        _solarClient = new SolarClient(options);
    }
        
    private void SetState(State state, string message = null)
    {
        if (_state != state)
        {
            _logger.LogInformation($"State of '{_options.Name}' transitioned from {_state} to {state}. {message}");
            _state = state;
        }
    }

    private string GetTopicState() => $"solis_scraper/sensor/{_options.NodeId}/state";

    private string GetTopicConfig(string name) => $"{_discoveryPrefix}/sensor/{_options.NodeId}/{name}/config";
        
    private async Task SendEntityConfigurations(CancellationToken cancellationToken)
    {
        var device = new HassDevice
        {
            Identifiers =
            [
                _options.NodeId
            ],
            Name = _options.Name
        };

        await RetryUntilSuccess(() =>
            _mqttClient.Send(GetTopicConfig("solis-now"), new HassConfig
            {
                DeviceClass = "power",
                Name = "Production (now)",
                StateTopic = GetTopicState(),
                UnitOfMeasurement = "W",
                ValueTemplate = "{{ value_json.watt_now }}",
                StateClass = "measurement",
                ForceUpdate = true,
                Icon = "mdi:solar-power",
                Device = device,
                UniqueId = $"{_options.UniqueId}_now"
            },
            cancellationToken: cancellationToken)
        );

        await RetryUntilSuccess(() =>
            _mqttClient.Send(GetTopicConfig("solis-today"), new HassConfig
            {
                DeviceClass = "energy",
                Name = "Production (today)",
                StateTopic = GetTopicState(),
                UnitOfMeasurement = "kWh",
                ValueTemplate = "{{ value_json.kilo_watt_today }}",
                StateClass = "total_increasing",
                ForceUpdate = true,
                Icon = "mdi:solar-power",
                Device = device,
                UniqueId = $"{_options.UniqueId}_today"
            },
            cancellationToken: cancellationToken)
        );

        await RetryUntilSuccess(() =>
            _mqttClient.Send(GetTopicConfig("solis-total"), new HassConfig
            {
                DeviceClass = "energy",
                Name = "Production (total)",
                StateTopic = GetTopicState(),
                UnitOfMeasurement = "kWh",
                ValueTemplate = "{{ value_json.kilo_watt_total }}",
                StateClass = "total_increasing",
                ForceUpdate = true,
                Icon = "mdi:solar-power",
                Device = device,
                UniqueId = $"{_options.UniqueId}_total"
            },
            cancellationToken: cancellationToken)
        );
    }
        
    public async Task Run(CancellationToken stoppingToken)
    {
        var didSetup = false;
        var lastState = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Scrape status page of remote.
                var (result, @continue) = await Scrape(stoppingToken);
                if (@continue) continue;

                result = await PostProcessResult(stoppingToken, result);

                if (result == null)
                {
                    continue;
                }

                // Filter duplicates to reduce state changes
                if (result.Equals(_previousResult))
                {
                    // when state did not change since last loop, check when last state was sent. if it
                    // was less than IntervalDuplicateState ago, do not send the state during this loop.
                    if (DateTime.UtcNow - lastState < _options.IntervalDuplicateState)
                    {
                        await Task.Delay(_options.IntervalZero, stoppingToken);
                        continue;
                    }
                }
                    
                // Send state to mqtt.
                await _mqttClient.Send(GetTopicState(), result, false, stoppingToken);
                _previousResult = result;
                lastState = DateTime.UtcNow;
                    
                // Send configuration of entities to mqtt after the initial state.
                if (!didSetup)
                {
                    await SendEntityConfigurations(stoppingToken);
                    didSetup = true;
                }

                // Sleep for next scrape cycle.
                SetState(State.Running);
                await Task.Delay(result.WattNow > 0 ? _options.IntervalValue : _options.IntervalZero, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                SetState(State.Unknown, e.Message);
                await Task.Delay(_options.IntervalError, stoppingToken);
            }
            catch (Exception e)
            {
                SetState(State.MqttUnavailable, e.Message);
                _logger.LogError(e, "Publishing failed");
                await Task.Delay(_options.IntervalError, stoppingToken);
            }
        }
    }

    private async Task<SolarScrapeResult> PostProcessResult(CancellationToken stoppingToken, SolarScrapeResult result)
    {
        switch (result)
        {
            case
            {
                KiloWattToday: 0,
                WattNow: 0,
                KiloWattTotal: 0
            }:
                // Sometimes the scraped page shows all zeroes. Skip these results.
                result = null;
                break;

            // When no new result could be scraped, assume the remote is sleeping due to no generation. Assume the previous result with a current watt value of 0.
            case null when !_sleepResultSent && _previousResult != null:
                {
                    result = _previousResult;
                    result.WattNow = 0;

                    // Reset the today stat after midnight (between 0:00 and 5:00)
                    if (!string.IsNullOrWhiteSpace(_options.ResetAfterMidnightTimeZone))
                    {
                        var localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, _options.ResetAfterMidnightTimeZone);
                        if (localTime.Hour is >= 0 and < 5)
                        {
                            result.KiloWattToday = 0;
                        }
                    }

                    _sleepResultSent = true;
                    break;
                }
            case null:
                await Task.Delay(_options.IntervalZero, stoppingToken);
                break;
            default:
                _sleepResultSent = false;
                break;
        }

        return result;
    }

    private async Task<(SolarScrapeResult result, bool @continue)> Scrape(CancellationToken stoppingToken)
    {
        try
        {
            var result = await _solarClient.Scrape(_format, stoppingToken);
            _failures = 0;
            return (result, false);
        }
        catch (HttpRequestException)
        {
            // remote is offline, probably due to lack of power generation
            return (null, false);
        }
        catch (ResponseParseException e)
        {
            // Cycle to next parse format and immediately retry until FailureCap is reached. At that point wait before retrying.
            SetState(State.SolarBadReply, e.Message);
            NextFormat();

            if (_failures++ < _options.FailureCap)
            {
                await Task.Delay(_options.IntervalError, stoppingToken);
                return (null, true);
            }
        }
        catch (TaskCanceledException e)
        {
            if (e.CancellationToken == stoppingToken)
            {
                return (null, true);
            }

            // A timeout has occurred. The inverter is probably offline. Retry until the FailureCap is reached. At that point wait before retrying.
            SetState(State.SolarUnavailable, e.Message);
            if (_failures++ < _options.FailureCap)
            {
                await Task.Delay(_options.IntervalError, stoppingToken);
                return (null, true);
            }
        }

        return (null, false);
    }
        
    private void NextFormat()
    {
        _format = _format == 1 ? 2 : 1;
        _logger.LogInformation($"Switching device '{_options.Name}' to format {_format}");
    }

    private async Task RetryUntilSuccess(Func<Task> action)
    {
        while (true)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Action failed, retrying...");
                await Task.Delay(1000);
            }
        }
    }


    private enum State
    {
        Initial,
        SolarUnavailable,
        SolarBadReply,
        MqttUnavailable,
        Running,
        Unknown
    }
}