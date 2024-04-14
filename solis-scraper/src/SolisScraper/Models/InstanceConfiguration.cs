using System;

namespace SolisScraper.Models
{
    public class InstanceConfiguration
    {
        // Solis
        public string Host { get; set; }
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public string ResetAfterMidnightTimeZone { get; set; } = null;
        public int Format { get; set; } = 1;
        public string Name { get; set; } = "Solis Energy";
        public int FailureCap { get; set; } = 5;

        // MQTT
        public string NodeId { get; set; } = "solis";
        public string UniqueId { get; set; } = "solis_scraper";

        // Timing
        public TimeSpan IntervalError { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan IntervalZero { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan IntervalValue { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan IntervalDuplicateState { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}