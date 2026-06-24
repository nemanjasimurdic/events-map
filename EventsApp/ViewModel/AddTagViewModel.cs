using System.Collections.Generic;
using System.ComponentModel;

namespace EventsApp.ViewModel
{
    public class ColorComboItem
    {
        public string DisplayName { get; set; }
        public string Hex         { get; set; }
    }

    public class AddTagViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private string _idText = "";
        public string IdText
        {
            get => _idText;
            set { _idText = value ?? ""; Notify(nameof(IdText)); }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set { _description = value ?? ""; Notify(nameof(Description)); }
        }

        private ColorComboItem _selectedColor;
        public ColorComboItem SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; Notify(nameof(SelectedColor)); }
        }

        public List<ColorComboItem> ColorItems { get; } = new List<ColorComboItem>
        {
            new ColorComboItem { DisplayName = "Green",  Hex = "#4CAF50" },
            new ColorComboItem { DisplayName = "Blue",   Hex = "#2196F3" },
            new ColorComboItem { DisplayName = "Amber",  Hex = "#FF9800" },
            new ColorComboItem { DisplayName = "Red",    Hex = "#F44336" },
            new ColorComboItem { DisplayName = "Purple", Hex = "#9C27B0" },
            new ColorComboItem { DisplayName = "Teal",   Hex = "#009688" },
            new ColorComboItem { DisplayName = "Indigo", Hex = "#3F51B5" },
        };

        public AddTagViewModel()
        {
            SelectedColor = ColorItems[0];
        }
    }
}
