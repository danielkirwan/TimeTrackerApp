using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTrackerApp.Models
{
    public class BreakPeriod
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class WorkSession
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();

        public Guid ClientId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public List<BreakPeriod> Breaks { get; set; } = new();

        public decimal HourlyRate { get; set; }

        public string CurrencySymbol { get; set; } = "£";

        public string Notes { get; set; } = string.Empty;

        public TimeSpan GetTotalBreakTime()
        {
            if (Breaks == null || Breaks.Count == 0) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(Breaks.Sum(b => (b.End - b.Start).TotalSeconds));
        }

        public TimeSpan GetWorkedTime()
        {
            if (EndTime == null) return TimeSpan.Zero;
            var total = EndTime.Value - StartTime;
            var breakTime = GetTotalBreakTime();
            if (breakTime > total) breakTime = total;
            return total - breakTime;
        }

        public decimal GetPay()
        {
            return (decimal)GetWorkedTime().TotalHours * HourlyRate;
        }

        public string WorkedTimeFormatted => FormatTimeSpan(GetWorkedTime());
        public string BreakTimeFormatted => FormatTimeSpan(GetTotalBreakTime());
        public string PayFormatted => $"{CurrencySymbol}{GetPay():0.00}";

        private static string FormatTimeSpan(TimeSpan ts)
        {
            return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}";
        }
    }
}
