using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class TagRowItem
    {
        public int    Code         { get; set; }
        public string Description  { get; set; }
        public string ColorHex     { get; set; }
        public string ColorDisplay { get; set; }
    }

    public class TagsViewModel : INotifyPropertyChanged
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

        public ObservableCollection<TagRowItem> Tags { get; }
            = new ObservableCollection<TagRowItem>();

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

        public TagsViewModel()
        {
            Load();
            _view = CollectionViewSource.GetDefaultView(Tags);
            _view.Filter = FilterPredicate;
        }

        private bool FilterPredicate(object obj)
        {
            if (string.IsNullOrEmpty(_filterText)) return true;
            var item = (TagRowItem)obj;
            return Contains(item.Description);
        }

        private bool Contains(string value) =>
            value != null &&
            value.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;

        private void Load()
        {
            var svc = new EventService();
            foreach (var t in svc.LoadTags())
            {
                string name = ColorNames.TryGetValue(t.ColorHex, out var n) ? n : t.ColorHex;
                Tags.Add(new TagRowItem
                {
                    Code         = t.Id,
                    Description  = t.Description,
                    ColorHex     = t.ColorHex,
                    ColorDisplay = $"{name} ({t.ColorHex})"
                });
            }
        }
    }
}
