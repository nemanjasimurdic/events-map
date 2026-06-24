using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class ChartBarItem
    {
        public string Label    { get; set; }
        public int    Count    { get; set; }
        public double BarWidth { get; set; }
    }

    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private const double MaxBarWidth = 150.0;

        public ObservableCollection<ChartBarItem> EventsPerType      { get; }
        public ObservableCollection<ChartBarItem> EventsByAttendance { get; }

        public double HumanitarianPercent     { get; }
        public int    HumanitarianCount       { get; }
        public int    NonHumanitarianCount    { get; }
        public string HumanitarianLabel       { get; }
        public string NonHumanitarianLabel    { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public StatisticsViewModel()
        {
            var service    = new EventService();
            var events     = service.LoadEvents();
            var eventTypes = service.LoadEventTypes();

            // events per type
            var typeLookup = eventTypes.ToDictionary(t => t.Id, t => t.Name);
            var byType = events
                .GroupBy(e => e.EventTypeId)
                .Select(g =>
                {
                    string name;
                    if (!typeLookup.TryGetValue(g.Key, out name)) name = "Unknown";
                    return new { Name = name, Count = g.Count() };
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            int maxType = byType.Count > 0 ? byType.Max(x => x.Count) : 1;
            EventsPerType = new ObservableCollection<ChartBarItem>(
                byType.Select(x => new ChartBarItem
                {
                    Label    = x.Name,
                    Count    = x.Count,
                    BarWidth = (x.Count / (double)maxType) * MaxBarWidth
                }));

            // event purpose
            HumanitarianCount    = events.Count(e => e.IsHumanitarian);
            NonHumanitarianCount = events.Count - HumanitarianCount;
            HumanitarianPercent  = events.Count > 0
                ? HumanitarianCount / (double)events.Count
                : 0;

            double nonPct = 1.0 - HumanitarianPercent;
            HumanitarianLabel    = $"Humanitarian — {HumanitarianCount} ({HumanitarianPercent:P0})";
            NonHumanitarianLabel = $"Standard — {NonHumanitarianCount} ({nonPct:P0})";

            // events by attendance
            var attendanceLabels = new Dictionary<AttendanceRange, string>
            {
                { AttendanceRange.Upto1000,        "Up to 1,000"     },
                { AttendanceRange.From1000To5000,  "1,000 – 5,000"   },
                { AttendanceRange.From5000To10000, "5,000 – 10,000"  },
                { AttendanceRange.Over10000,        "Over 10,000"     },
            };

            var attendanceCounts = events
                .GroupBy(e => e.Attendance)
                .ToDictionary(g => g.Key, g => g.Count());

            int maxAtt = attendanceCounts.Count > 0 ? attendanceCounts.Values.Max() : 1;

            EventsByAttendance = new ObservableCollection<ChartBarItem>(
                Enum.GetValues(typeof(AttendanceRange))
                    .Cast<AttendanceRange>()
                    .Select(range =>
                    {
                        int count;
                        if (!attendanceCounts.TryGetValue(range, out count)) count = 0;
                        string label;
                        if (!attendanceLabels.TryGetValue(range, out label)) label = range.ToString();
                        return new ChartBarItem
                        {
                            Label    = label,
                            Count    = count,
                            BarWidth = maxAtt > 0 ? (count / (double)maxAtt) * MaxBarWidth : 0
                        };
                    }));
        }
    }
}
