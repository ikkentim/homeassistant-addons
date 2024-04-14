using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolisScraper.Models;

namespace SolisScraper
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (OptionsValidationException e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(cfg =>
                {
                    cfg.AddSimpleConsole(con =>
                    {
                        con.SingleLine = true;
                        con.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                    });
                })
#if DEBUG
                .UseEnvironment("Debug")
#endif
                .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
                .ConfigureServices((ctx, services) =>
                {
                    services.AddOptions<ScraperConfiguration>()
                        .Bind(ctx.Configuration.GetSection("Scraper"))
                        .Validate(v =>
                        {
                            foreach (var instance in v.Instances)
                            {
                                if (
                                    string.IsNullOrEmpty(instance.Host) ||
                                    string.IsNullOrEmpty(instance.Username) ||
                                    string.IsNullOrEmpty(instance.Password) ||
                                    instance.Format < 1 || instance.Format > 2 ||
                                    string.IsNullOrEmpty(instance.Name) || 
                                    string.IsNullOrEmpty(instance.NodeId) || 
                                    string.IsNullOrEmpty(instance.UniqueId)
                                    )
                                {
                                    return false;
                                }
                            }

                            return true;
                        }, "Scraper instances configuration is invalid.");

                    services.AddOptions<MqttConfiguration>()
                        .Bind(ctx.Configuration.GetSection("Mqtt"))
                        .Validate(v => !string.IsNullOrEmpty(v.Host), Message("Host"))
                        .Validate(v => !string.IsNullOrEmpty(v.Username), Message("Username"))
                        .Validate(v => !string.IsNullOrEmpty(v.Password), Message("Password"))
                        .Validate(v => !string.IsNullOrEmpty(v.ClientId), Message("ClientId"))
                        .Validate(v => !string.IsNullOrEmpty(v.DiscoveryPrefix), Message("DiscoveryPrefix"))
                        ;
                    
                    services.AddTransient<MqttTransmitter>();
                    services.AddHostedService<ScraperBackgroundService>();
                    
                    static string Message(string what)
                    {
                        return $"Missing MQTT {what.ToLowerInvariant()}. Configure using Mqtt.{what} in appsettings.json or using the Mqtt__{what} environment variable.";
                    }

                });


    }
}
