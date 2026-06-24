using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using EventsApp.Models;
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
                { "#9C27B0", "Purple" },
                { "#009688", "Teal"   },
                { "#3F51B5", "Indigo" }
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

        public SearchTagViewModel SearchState { get; } = new SearchTagViewModel();

        public TagsViewModel()
        {
            Load();
            _view = CollectionViewSource.GetDefaultView(Tags);
            _view.Filter = FilterPredicate;
        }

        private Func<TagRowItem, bool> _searchPredicate;

        private bool FilterPredicate(object obj)
        {
            var item = (TagRowItem)obj;
            if (!string.IsNullOrEmpty(_filterText))
            {
                if (!Contains(item.Description))
                    return false;
            }
            return _searchPredicate == null || _searchPredicate(item);
        }

        public void ApplySearch(Func<TagRowItem, bool> predicate)
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

        public void AddRow(Tag tag)
        {
            string name = ColorNames.TryGetValue(tag.ColorHex, out var n) ? n : tag.ColorHex;
            Tags.Add(new TagRowItem
            {
                Code         = tag.Id,
                Description  = tag.Description,
                ColorHex     = tag.ColorHex,
                ColorDisplay = $"{name} ({tag.ColorHex})"
            });
        }

        public void UpdateRow(int originalId, Tag tag)
        {
            string name = ColorNames.TryGetValue(tag.ColorHex, out var n) ? n : tag.ColorHex;
            for (int i = 0; i < Tags.Count; i++)
            {
                if (Tags[i].Code == originalId)
                {
                    Tags[i] = new TagRowItem
                    {
                        Code         = tag.Id,
                        Description  = tag.Description,
                        ColorHex     = tag.ColorHex,
                        ColorDisplay = $"{name} ({tag.ColorHex})"
                    };
                    return;
                }
            }
        }

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
