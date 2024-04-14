using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolisScraper.Models;

namespace SolisScraper
{
    public class ScraperBackgroundService : BackgroundService
    {
        private readonly MqttTransmitter _mqttClient;
        private readonly ILogger _logger;
        private readonly List<ScraperInstance> _instances = [];

        public ScraperBackgroundService(MqttTransmitter mqttClient,
            IOptions<MqttConfiguration> options, 
            IOptions<ScraperConfiguration> scraperOptions,
            ILogger<ScraperBackgroundService> logger, 
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _mqttClient = mqttClient;

            var instanceLogger = loggerFactory.CreateLogger<ScraperInstance>();
            foreach (var instance in scraperOptions.Value.Instances)
            {
                _instances.Add(new ScraperInstance(_mqttClient, options.Value.DiscoveryPrefix, instance, instanceLogger));
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RetryUntilSuccess(() => _mqttClient.Start());

            var runs = _instances.Select(x => x.Run(stoppingToken))
                .ToList();

            await Task.WhenAll(runs);

            await _mqttClient.Stop();
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
    }
}