namespace SolisScraper.Models
{
    public class MqttConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public string ClientId { get; set; } = "solis_scraper";
        public bool DebugLogging { get; set; }
        public string DiscoveryPrefix { get; set; } = "homeassistant";

        public bool Dummy { get; set; }
    }
}