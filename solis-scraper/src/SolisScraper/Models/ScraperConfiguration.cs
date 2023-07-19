using System;

namespace SolisScraper.Models
{
    public class ScraperConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public int Format { get; set; } = 1;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}