using System.Collections.ObjectModel;
using System.ComponentModel;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventTypeRowItem
    {
        public string IconPath { get; set; }
        public int    Code     { get; set; }
        public string Name     { get; set; }
    }

    public class EventTypesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventTypeRowItem> EventTypes { get; }
            = new ObservableCollection<EventTypeRowItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public EventTypesViewModel()
        {
            Load();
        }

        private void Load()
        {
            var svc  = new EventService();
            const string pack = "pack://application:,,,/EventsApp;component/";

            foreach (var t in svc.LoadEventTypes())
            {
                EventTypes.Add(new EventTypeRowItem
                {
                    IconPath = t.IconPath != null
                        ? pack + t.IconPath
                        : pack + "Resources/Images/world-map.png",
                    Code = t.Id,
                    Name = t.Name
                });
            }
        }
    }
}
