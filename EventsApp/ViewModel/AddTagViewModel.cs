using System.Collections.Generic;
using System.Linq;

namespace EventsApp.ViewModel
{
    public class ColorComboItem
    {
        public string DisplayName { get; set; }
        public string Hex         { get; set; }
    }

    public class AddTagViewModel : ViewModelBase
    {
        private string _idText = "";
        public string IdText
        {
            get => _idText;
            set => SetProperty(ref _idText, value ?? "");
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value ?? "");
        }

        private ColorComboItem _selectedColor;
        public ColorComboItem SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }


        public bool   IdHasError    => GetErrors(nameof(IdText)).Cast<string>().Any();
        public bool   ColorHasError => GetErrors(nameof(SelectedColor)).Cast<string>().Any();

        public string IdError       => GetErrors(nameof(IdText)).Cast<string>().FirstOrDefault() ?? "";
        public string ColorError    => GetErrors(nameof(SelectedColor)).Cast<string>().FirstOrDefault() ?? "";

        public void SetIdError(string message) => SetErrors(nameof(IdText), new[] { message });


        protected override void ValidateProperty(object value, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(IdText):
                    var idText = value as string ?? "";
                    if (string.IsNullOrWhiteSpace(idText))
                        SetErrors(propertyName, new[] { "Required field" });
                    else if (!int.TryParse(idText.Trim(), out int parsed) || parsed <= 0)
                        SetErrors(propertyName, new[] { "Must be a positive number" });
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(SelectedColor):
                    var color = value as ColorComboItem;
                    if (color == null || color.Hex == null)
                        SetErrors(propertyName, new[] { "Required field" });
                    else
                        ClearErrors(propertyName);
                    break;
            }
        }


        public List<ColorComboItem> ColorItems { get; } = new List<ColorComboItem>
        {
            new ColorComboItem { DisplayName = "(Select color)", Hex = null     },
            new ColorComboItem { DisplayName = "Green",          Hex = "#4CAF50" },
            new ColorComboItem { DisplayName = "Blue",   Hex = "#2196F3" },
            new ColorComboItem { DisplayName = "Amber",  Hex = "#FF9800" },
            new ColorComboItem { DisplayName = "Red",    Hex = "#F44336" },
            new ColorComboItem { DisplayName = "Purple", Hex = "#9C27B0" },
            new ColorComboItem { DisplayName = "Teal",   Hex = "#009688" },
            new ColorComboItem { DisplayName = "Indigo", Hex = "#3F51B5" },
        };


        public AddTagViewModel()
        {
            _selectedColor = ColorItems[0];

            ErrorsChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IdText))
                {
                    OnPropertyChanged(nameof(IdHasError));
                    OnPropertyChanged(nameof(IdError));
                }
                else if (e.PropertyName == nameof(SelectedColor))
                {
                    OnPropertyChanged(nameof(ColorHasError));
                    OnPropertyChanged(nameof(ColorError));
                }
            };
        }
    }
}
