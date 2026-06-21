using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventCardItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string DateText { get; set; }
        public string IconPath { get; set; }

        private bool _isVisibleOnMap = true;
        public bool IsVisibleOnMap
        {
            get => _isVisibleOnMap;
            set
            {
                if (_isVisibleOnMap == value) return;
                _isVisibleOnMap = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisibleOnMap)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly EventService _eventService = new EventService();

        public ObservableCollection<EventCardItem> EventItems { get; set; }
        public ObservableCollection<EventCardItem> MapMarkerItems { get; set; }

        private string _mapFilterText = "";
        public string MapFilterText
        {
            get => _mapFilterText;
            set
            {
                _mapFilterText = value ?? "";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MapFilterText)));
                ApplyMapFilter();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            EventItems     = new ObservableCollection<EventCardItem>();
            MapMarkerItems = new ObservableCollection<EventCardItem>();
            // Re-apply filter whenever a marker is added to the map
            MapMarkerItems.CollectionChanged += (s, e) => ApplyMapFilter();
            LoadEvents();
        }

        private void ApplyMapFilter()
        {
            foreach (var item in MapMarkerItems)
                item.IsVisibleOnMap = string.IsNullOrEmpty(_mapFilterText) ||
                    item.Name.IndexOf(_mapFilterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LoadEvents()
        {
            try
            {
                var typeIcons = new Dictionary<int, string>();
                foreach (var t in _eventService.LoadEventTypes())
                    typeIcons[t.Id] = "/" + t.IconPath;

                foreach (var ev in _eventService.LoadEvents())
                {
                    string iconPath = null;
                    if (ev.IconPath != null)
                        iconPath = "/" + ev.IconPath.TrimStart('/')
                            .Replace("Resources/Icons/", "Resources/Images/");
                    else if (typeIcons.ContainsKey(ev.EventTypeId))
                        iconPath = typeIcons[ev.EventTypeId];

                    string dateText = ev.CurrentYearDate.HasValue
                        ? ev.CurrentYearDate.Value.ToString("yyyy-MM-dd")
                        : "TBD";

                    EventItems.Add(new EventCardItem
                    {
                        Name     = ev.Name,
                        Location = $"{ev.City}, {ev.Country}",
                        DateText = dateText,
                        IconPath = iconPath ?? "/Resources/Images/world-map.png"
                    });
                }
            }
            catch (Exception) { }
        }
    }
}
