using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventRowItem
    {
        public string IconPath { get; set; }
        public int    EventId  { get; set; }
        public string Name     { get; set; }
        public string TypeName { get; set; }
        public string Location { get; set; }
    }

    public class EventsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventRowItem> Events { get; }
            = new ObservableCollection<EventRowItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public EventsViewModel()
        {
            Load();
        }

        private void Load()
        {
            var svc     = new EventService();
            var typeMap = new Dictionary<int, EventType>();
            foreach (var t in svc.LoadEventTypes())
                typeMap[t.Id] = t;

            const string pack = "pack://application:,,,/EventsApp;component/";

            foreach (var ev in svc.LoadEvents())
            {
                string iconPath = null;
                if (ev.IconPath != null)
                    iconPath = pack + ev.IconPath.TrimStart('/')
                                          .Replace("Resources/Icons/", "Resources/Images/");
                else if (typeMap.ContainsKey(ev.EventTypeId) && typeMap[ev.EventTypeId].IconPath != null)
                    iconPath = pack + typeMap[ev.EventTypeId].IconPath;

                var typeName = typeMap.ContainsKey(ev.EventTypeId)
                    ? typeMap[ev.EventTypeId].Name : "";

                Events.Add(new EventRowItem
                {
                    IconPath = iconPath ?? pack + "Resources/Images/world-map.png",
                    EventId  = ev.Id,
                    Name     = ev.Name,
                    TypeName = typeName,
                    Location = $"{ev.City}, {ev.Country}"
                });
            }
        }
    }
}
