using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EventsApp.Models;

namespace EventsApp.ViewModel
{
    public class EditEventTypeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public int OriginalId { get; }

        private string _idText = "";
        public string IdText
        {
            get => _idText;
            set { _idText = value ?? ""; Notify(nameof(IdText)); }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value ?? ""; Notify(nameof(Name)); }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set { _description = value ?? ""; Notify(nameof(Description)); }
        }

        private IconComboItem _selectedIcon;
        public IconComboItem SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;
                Notify(nameof(SelectedIcon));
                Notify(nameof(IconPreviewPath));
            }
        }

        public string IconPreviewPath => _selectedIcon?.PreviewPath;

        public List<IconComboItem> IconItems { get; }

        public EditEventTypeViewModel(EventType et)
        {
            OriginalId = et.Id;

            const string pack = "pack://application:,,,/EventsApp;component/";

            IconItems = new List<IconComboItem>
            {
                new IconComboItem { DisplayName = "(None)",      FilePath = null,                               PreviewPath = null },
                new IconComboItem { DisplayName = "Event type",  FilePath = "Resources/Images/event-type.png",  PreviewPath = pack + "Resources/Images/event-type.png" },
                new IconComboItem { DisplayName = "Film",        FilePath = "Resources/Images/film.png",        PreviewPath = pack + "Resources/Images/film.png" },
                new IconComboItem { DisplayName = "Music",       FilePath = "Resources/Images/music.png",       PreviewPath = pack + "Resources/Images/music.png" },
                new IconComboItem { DisplayName = "Tennis",      FilePath = "Resources/Images/tennis.png",      PreviewPath = pack + "Resources/Images/tennis.png" },
                new IconComboItem { DisplayName = "Basketball",  FilePath = "Resources/Images/basketball.png",  PreviewPath = pack + "Resources/Images/basketball.png" },
                new IconComboItem { DisplayName = "Art",         FilePath = "Resources/Images/art.png",         PreviewPath = pack + "Resources/Images/art.png" },
            };

            IdText       = et.Id.ToString();
            Name         = et.Name ?? "";
            Description  = et.Description ?? "";
            SelectedIcon = IconItems.FirstOrDefault(i => i.FilePath == et.IconPath) ?? IconItems[0];
        }
    }
}
