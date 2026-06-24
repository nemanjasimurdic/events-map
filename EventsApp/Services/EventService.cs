using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using EventsApp.Models;

namespace EventsApp.Services
{
    public class EventService
    {
        private static readonly string DataDir =
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Data"));
        private static readonly string EventsPath = Path.Combine(DataDir, "events.json");
        private static readonly string TypesPath  = Path.Combine(DataDir, "eventTypes.json");
        private static readonly string TagsPath   = Path.Combine(DataDir, "tags.json");

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

        public List<Tag> LoadTags()
        {
            if (!File.Exists(TagsPath)) return new List<Tag>();

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<List<Tag>>(File.ReadAllText(TagsPath));
        }

        public void SaveEvents(List<Event> events)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var ev in events)
            {
                List<string> histDates = null;
                if (ev.HistoryDates != null)
                {
                    histDates = new List<string>();
                    foreach (var d in ev.HistoryDates)
                        histDates.Add(d.ToString("yyyy-MM-dd"));
                }

                list.Add(new Dictionary<string, object>
                {
                    { "Id",              ev.Id                  },
                    { "Name",            ev.Name                },
                    { "Description",     ev.Description         },
                    { "EventTypeId",     ev.EventTypeId         },
                    { "Attendance",      (int)ev.Attendance     },
                    { "IconPath",        (object)ev.IconPath    },
                    { "IsHumanitarian",  ev.IsHumanitarian      },
                    { "AverageCost",     ev.AverageCost         },
                    { "Country",         ev.Country             },
                    { "City",            ev.City                },
                    { "HistoryDates",    (object)histDates      },
                    { "CurrentYearDate", ev.CurrentYearDate.HasValue
                                            ? (object)ev.CurrentYearDate.Value.ToString("yyyy-MM-dd")
                                            : null             },
                    { "TagIds",          (object)ev.TagIds      },
                    { "MapX",            ev.MapX.HasValue ? (object)ev.MapX.Value : null },
                    { "MapY",            ev.MapY.HasValue ? (object)ev.MapY.Value : null },
                });
            }
            var serializer = new JavaScriptSerializer();
            File.WriteAllText(EventsPath, serializer.Serialize(list));
        }

        public void SaveEventTypes(List<EventType> types)
        {
            var serializer = new JavaScriptSerializer();
            File.WriteAllText(TypesPath, serializer.Serialize(types));
        }

        public void SaveTags(List<Tag> tags)
        {
            var serializer = new JavaScriptSerializer();
            File.WriteAllText(TagsPath, serializer.Serialize(tags));
        }
    }
}
