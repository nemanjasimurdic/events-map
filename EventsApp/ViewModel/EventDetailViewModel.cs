using System;
using System.Collections.Generic;
using System.Linq;
using EventsApp.Models;

namespace EventsApp.ViewModel
{
    public class TagChipItem
    {
        public string Label { get; set; }
    }

    public class EventDetailViewModel
    {
        private static readonly Dictionary<string, string> ColorNames =
            new Dictionary<string, string>
            {
                { "#4CAF50", "Green"  },
                { "#2196F3", "Blue"   },
                { "#FF9800", "Amber"  },
                { "#F44336", "Red"    },
                { "#9C27B0", "Purple" }
            };

        public string  IconPath        { get; }
        public int     EventId         { get; }
        public string  Name            { get; }
        public string  Description     { get; }
        public string  TypeIconPath    { get; }
        public string  TypeName        { get; }
        public string  Location        { get; }
        public string  CurrentDateText { get; }
        public string  AttendanceText  { get; }
        public string  AverageCostText { get; }
        public bool    IsHumanitarian  { get; }
        public List<string>      HistoryDateTexts { get; }
        public List<TagChipItem> Tags             { get; }

        public EventDetailViewModel(Event ev, List<EventType> types, List<Tag> allTags)
        {
            const string pack = "pack://application:,,,/EventsApp;component/";

            var type = types.FirstOrDefault(t => t.Id == ev.EventTypeId);

            string iconPath = null;
            if (ev.IconPath != null)
                iconPath = pack + ev.IconPath.TrimStart('/')
                               .Replace("Resources/Icons/", "Resources/Images/");
            else if (type != null && type.IconPath != null)
                iconPath = pack + type.IconPath;
            IconPath = iconPath ?? pack + "Resources/Images/world-map.png";

            EventId     = ev.Id;
            Name        = ev.Name;
            Description = ev.Description;

            TypeIconPath = (type != null && type.IconPath != null)
                ? pack + type.IconPath
                : pack + "Resources/Images/event-type.png";
            TypeName = type != null ? type.Name : "Unknown";

            Location        = string.Format("{0}, {1}", ev.City, ev.Country);
            CurrentDateText = ev.CurrentYearDate.HasValue
                ? ev.CurrentYearDate.Value.ToString("MMMM d, yyyy")
                : "Date TBD";

            AttendanceText  = AttendanceToString(ev.Attendance);
            AverageCostText = ev.AverageCost == 0.0 ? "Free" : string.Format("${0:F2}", ev.AverageCost);
            IsHumanitarian  = ev.IsHumanitarian;

            HistoryDateTexts = ev.HistoryDates != null
                ? ev.HistoryDates
                      .OrderByDescending(d => d)
                      .Select(d => d.ToString("MMMM d, yyyy"))
                      .ToList()
                : new List<string>();

            var tagLookup = allTags.ToDictionary(t => t.Id.ToString());
            Tags = ev.TagIds != null
                ? ev.TagIds
                      .Where(id => tagLookup.ContainsKey(id))
                      .Select(id =>
                      {
                          var tag = tagLookup[id];
                          string colorName;
                          if (!ColorNames.TryGetValue(tag.ColorHex, out colorName))
                              colorName = tag.ColorHex;
                          return new TagChipItem
                          {
                              Label = string.Format("{0} · {1}", tag.Description, colorName)
                          };
                      })
                      .ToList()
                : new List<TagChipItem>();
        }

        private static string AttendanceToString(AttendanceRange range)
        {
            switch (range)
            {
                case AttendanceRange.Upto1000:        return "Up to 1,000";
                case AttendanceRange.From1000To5000:  return "1,000 – 5,000";
                case AttendanceRange.From5000To10000: return "5,000 – 10,000";
                case AttendanceRange.Over10000:       return "Over 10,000";
                default:                              return range.ToString();
            }
        }
    }
}
