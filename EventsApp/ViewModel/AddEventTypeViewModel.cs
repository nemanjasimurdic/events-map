using System.Collections.Generic;
using System.Linq;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class AddEventTypeViewModel : ViewModelBase
    {
        private string _idText = "";
        public string IdText
        {
            get => _idText;
            set => SetProperty(ref _idText, value ?? "");
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value ?? "");
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value ?? "");
        }

        private IconComboItem _selectedIcon;
        public IconComboItem SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                if (SetProperty(ref _selectedIcon, value))
                    OnPropertyChanged(nameof(IconPreviewPath));
            }
        }

        public string IconPreviewPath => _selectedIcon?.PreviewPath;

        // ── Validation helpers ───────────────────────────────────────────────

        public bool   IdHasError   => GetErrors(nameof(IdText)).Cast<string>().Any();
        public bool   NameHasError => GetErrors(nameof(Name)).Cast<string>().Any();
        public bool   IconHasError => GetErrors(nameof(SelectedIcon)).Cast<string>().Any();

        public string IdError      => GetErrors(nameof(IdText)).Cast<string>().FirstOrDefault() ?? "";
        public string NameError    => GetErrors(nameof(Name)).Cast<string>().FirstOrDefault() ?? "";
        public string IconError    => GetErrors(nameof(SelectedIcon)).Cast<string>().FirstOrDefault() ?? "";

        public void SetIdError(string message) => SetErrors(nameof(IdText), new[] { message });

        // ── Validation logic ─────────────────────────────────────────────────

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

                case nameof(Name):
                    var name = value as string ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                        SetErrors(propertyName, new[] { "Required field" });
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(SelectedIcon):
                    var icon = value as IconComboItem;
                    if (icon == null || icon.FilePath == null)
                        SetErrors(propertyName, new[] { "Required field" });
                    else
                        ClearErrors(propertyName);
                    break;
            }
        }

        // ── Collections ──────────────────────────────────────────────────────

        public List<IconComboItem> IconItems { get; }

        // ── Constructor ──────────────────────────────────────────────────────

        public AddEventTypeViewModel()
        {
            const string pack = "pack://application:,,,/EventsApp;component/";

            IconItems = new List<IconComboItem>
            {
                new IconComboItem { DisplayName = "(None)",       FilePath = null,                                PreviewPath = null },
                new IconComboItem { DisplayName = "Event type",   FilePath = "Resources/Images/event-type.png",  PreviewPath = pack + "Resources/Images/event-type.png" },
                new IconComboItem { DisplayName = "Film",         FilePath = "Resources/Images/film.png",         PreviewPath = pack + "Resources/Images/film.png" },
                new IconComboItem { DisplayName = "Music",        FilePath = "Resources/Images/music.png",        PreviewPath = pack + "Resources/Images/music.png" },
                new IconComboItem { DisplayName = "Tennis",       FilePath = "Resources/Images/tennis.png",       PreviewPath = pack + "Resources/Images/tennis.png" },
                new IconComboItem { DisplayName = "Basketball",   FilePath = "Resources/Images/basketball.png",   PreviewPath = pack + "Resources/Images/basketball.png" },
                new IconComboItem { DisplayName = "Art",          FilePath = "Resources/Images/art.png",          PreviewPath = pack + "Resources/Images/art.png" },
            };
            _selectedIcon = IconItems[0];

            ErrorsChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IdText))
                {
                    OnPropertyChanged(nameof(IdHasError));
                    OnPropertyChanged(nameof(IdError));
                }
                else if (e.PropertyName == nameof(Name))
                {
                    OnPropertyChanged(nameof(NameHasError));
                    OnPropertyChanged(nameof(NameError));
                }
                else if (e.PropertyName == nameof(SelectedIcon))
                {
                    OnPropertyChanged(nameof(IconHasError));
                    OnPropertyChanged(nameof(IconError));
                }
            };
        }
    }
}
