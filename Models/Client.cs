using System;

namespace TimeTrackerApp.Models
{
    public class Client
    {
        public Guid ClientId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public bool IsCompany { get; set; }

        public string CurrencySymbol { get; set; } = "£";

        public decimal HourlyRate { get; set; }

        public override string ToString()
        {
            var type = IsCompany ? "Company" : "Individual";
            return $"{Name} ({type})";
        }
    }
}
