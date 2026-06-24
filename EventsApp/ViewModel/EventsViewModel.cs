using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventRowItem
    {
        public string IconPath    { get; set; }
        public int    EventId     { get; set; }
        public string Name        { get; set; }
        public string TypeName    { get; set; }
        public string Location    { get; set; }
        public string Country     { get; set; }
        public string City        { get; set; }
        public string Description { get; set; }
    }

    public class EventsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventRowItem> Events { get; }
            = new ObservableCollection<EventRowItem>();

        private ICollectionView _view;

        private string _filterText = "";
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value ?? "";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterText)));
                _view.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SearchEventViewModel SearchState { get; }

        public EventsViewModel()
        {
            Load();
            SearchState = new SearchEventViewModel(new EventService().LoadEventTypes());
            _view = CollectionViewSource.GetDefaultView(Events);
            _view.Filter = FilterPredicate;
        }

        private Func<EventRowItem, bool> _searchPredicate;

        private bool FilterPredicate(object obj)
        {
            var item = (EventRowItem)obj;
            if (!string.IsNullOrEmpty(_filterText))
            {
                if (!Contains(item.Name)        &&
                    !Contains(item.TypeName)    &&
                    !Contains(item.Country)     &&
                    !Contains(item.City)        &&
                    !Contains(item.Description))
                    return false;
            }
            return _searchPredicate == null || _searchPredicate(item);
        }

        public void ApplySearch(Func<EventRowItem, bool> predicate)
        {
            _searchPredicate = predicate;
            _view.Refresh();
        }

        public void ClearSearch()
        {
            _searchPredicate = null;
            _view.Refresh();
        }

        private bool Contains(string value) =>
            value != null &&
            value.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;

        public void AddRow(Event ev, string typeName, string iconPath)
        {
            Events.Add(new EventRowItem
            {
                IconPath    = iconPath,
                EventId     = ev.Id,
                Name        = ev.Name,
                TypeName    = typeName,
                Location    = $"{ev.City}, {ev.Country}",
                Country     = ev.Country,
                City        = ev.City,
                Description = ev.Description
            });
        }

        public void UpdateRow(int originalId, Event ev, string typeName, string iconPath)
        {
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].EventId == originalId)
                {
                    Events[i] = new EventRowItem
                    {
                        IconPath    = iconPath,
                        EventId     = ev.Id,
                        Name        = ev.Name,
                        TypeName    = typeName,
                        Location    = $"{ev.City}, {ev.Country}",
                        Country     = ev.Country,
                        City        = ev.City,
                        Description = ev.Description
                    };
                    return;
                }
            }
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
                    ? typeMap[ev.EventTypeId].Name : "Unknown type";

                Events.Add(new EventRowItem
                {
                    IconPath    = iconPath ?? pack + "Resources/Images/world-map.png",
                    EventId     = ev.Id,
                    Name        = ev.Name,
                    TypeName    = typeName,
                    Location    = $"{ev.City}, {ev.Country}",
                    Country     = ev.Country,
                    City        = ev.City,
                    Description = ev.Description
                });
            }
        }
    }
}
