using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using EventsApp.Models;

namespace EventsApp.Services
{
    public class EventService
    {
        private static readonly string BaseDir    = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string EventsPath = Path.Combine(BaseDir, "Data", "events.json");
        private static readonly string TypesPath  = Path.Combine(BaseDir, "Data", "eventTypes.json");

        public List<Event> LoadEvents()
        {
            if (!File.Exists(EventsPath)) return new List<Event>();

            var serializer = new JavaScriptSerializer();
            var raw = serializer.Deserialize<List<Dictionary<string, object>>>(
                File.ReadAllText(EventsPath));

            var result = new List<Event>();
            foreach (var d in raw)
            {
                var ev = new Event
                {
                    Id             = (int)d["Id"],
                    Name           = d["Name"]?.ToString(),
                    Description    = d["Description"]?.ToString(),
                    EventTypeId    = (int)d["EventTypeId"],
                    Attendance     = (AttendanceRange)(int)d["Attendance"],
                    IconPath       = d["IconPath"]?.ToString(),
                    IsHumanitarian = (bool)d["IsHumanitarian"],
                    AverageCost    = Convert.ToDouble(d["AverageCost"]),
                    Country        = d["Country"]?.ToString(),
                    City           = d["City"]?.ToString(),
                    CurrentYearDate = d["CurrentYearDate"] != null
                        ? (DateTime?)DateTime.Parse(d["CurrentYearDate"].ToString())
                        : null,
                    MapX = d["MapX"] != null ? (double?)Convert.ToDouble(d["MapX"]) : null,
                    MapY = d["MapY"] != null ? (double?)Convert.ToDouble(d["MapY"]) : null,
                };

                if (d["HistoryDates"] is System.Collections.ArrayList histList)
                {
                    ev.HistoryDates = new List<DateTime>();
                    foreach (var item in histList)
                        ev.HistoryDates.Add(DateTime.Parse(item.ToString()));
                }

                if (d["TagIds"] is System.Collections.ArrayList tagList)
                {
                    ev.TagIds = new List<string>();
                    foreach (var item in tagList)
                        ev.TagIds.Add(item.ToString());
                }

                result.Add(ev);
            }
            return result;
        }

        public List<EventType> LoadEventTypes()
        {
            if (!File.Exists(TypesPath)) return new List<EventType>();

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<EventType>>(File.ReadAllText(TypesPath));
        }
    }
}
