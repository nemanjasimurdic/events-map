using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventTypeRowItem
    {
        public string IconPath    { get; set; }
        public int    Code        { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }
    }

    public class EventTypesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventTypeRowItem> EventTypes { get; }
            = new ObservableCollection<EventTypeRowItem>();

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

        public SearchEventTypeViewModel SearchState { get; } = new SearchEventTypeViewModel();

        public EventTypesViewModel()
        {
            Load();
            _view = CollectionViewSource.GetDefaultView(EventTypes);
            _view.Filter = FilterPredicate;
        }

        private Func<EventTypeRowItem, bool> _searchPredicate;

        private bool FilterPredicate(object obj)
        {
            var item = (EventTypeRowItem)obj;
            if (!string.IsNullOrEmpty(_filterText))
            {
                if (!Contains(item.Name) && !Contains(item.Description))
                    return false;
            }
            return _searchPredicate == null || _searchPredicate(item);
        }

        public void ApplySearch(Func<EventTypeRowItem, bool> predicate)
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

        public void AddRow(EventType et, string iconPath)
        {
            EventTypes.Add(new EventTypeRowItem
            {
                IconPath    = iconPath,
                Code        = et.Id,
                Name        = et.Name,
                Description = et.Description
            });
        }

        public void UpdateRow(int originalId, EventType et, string iconPath)
        {
            for (int i = 0; i < EventTypes.Count; i++)
            {
                if (EventTypes[i].Code == originalId)
                {
                    EventTypes[i] = new EventTypeRowItem
                    {
                        IconPath    = iconPath,
                        Code        = et.Id,
                        Name        = et.Name,
                        Description = et.Description
                    };
                    return;
                }
            }
        }

        private void Load()
        {
            var svc = new EventService();
            const string pack = "pack://application:,,,/EventsApp;component/";

            foreach (var t in svc.LoadEventTypes())
            {
                EventTypes.Add(new EventTypeRowItem
                {
                    IconPath    = t.IconPath != null
                        ? pack + t.IconPath
                        : pack + "Resources/Images/world-map.png",
                    Code        = t.Id,
                    Name        = t.Name,
                    Description = t.Description
                });
            }
        }
    }
}
