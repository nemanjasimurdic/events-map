using System;
using System.Collections.Generic;

namespace EventsApp.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int EventTypeId { get; set; }
        public AttendanceRange Attendance { get; set; }
        public string IconPath { get; set; }
        public bool IsHumanitarian { get; set; }
        public double AverageCost { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public List<DateTime> HistoryDates { get; set; }
        public DateTime? CurrentYearDate { get; set; }
        public List<string> TagIds { get; set; }
        public double? MapX { get; set; }
        public double? MapY { get; set; }
    }
}
