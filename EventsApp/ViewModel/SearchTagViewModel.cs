using System.Collections.Generic;
using System.ComponentModel;

namespace EventsApp.ViewModel
{
    public class SearchTagColorItem
    {
        public string DisplayName { get; set; }
        public string Hex         { get; set; }
    }

    public class SearchTagViewModel : INotifyPropertyChanged
    {
        public List<SearchTagColorItem> ColorItems { get; } = new List<SearchTagColorItem>
        {
            new SearchTagColorItem { DisplayName = "(Any)",  Hex = null      },
            new SearchTagColorItem { DisplayName = "Green",  Hex = "#4CAF50" },
            new SearchTagColorItem { DisplayName = "Blue",   Hex = "#2196F3" },
            new SearchTagColorItem { DisplayName = "Amber",  Hex = "#FF9800" },
            new SearchTagColorItem { DisplayName = "Red",    Hex = "#F44336" },
            new SearchTagColorItem { DisplayName = "Purple", Hex = "#9C27B0" },
            new SearchTagColorItem { DisplayName = "Teal",   Hex = "#009688" },
            new SearchTagColorItem { DisplayName = "Indigo", Hex = "#3F51B5" },
        };

        private SearchTagColorItem _selectedColor;
        public SearchTagColorItem SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; Notify(nameof(SelectedColor)); }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set { _description = value ?? ""; Notify(nameof(Description)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SearchTagViewModel()
        {
            Reset();
        }

        public void Reset()
        {
            Description   = "";
            SelectedColor = ColorItems[0];
        }

        private void Notify(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
