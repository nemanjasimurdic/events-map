using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EventsApp.Models;

namespace EventsApp.ViewModel
{
    public class EditEventViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public int OriginalId { get; }


        private string _eventIdText = "";
        public string EventIdText
        {
            get => _eventIdText;
            set { _eventIdText = value ?? ""; Notify(nameof(EventIdText)); }
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

        private EventTypeComboItem _selectedEventType;
        public EventTypeComboItem SelectedEventType
        {
            get => _selectedEventType;
            set { _selectedEventType = value; Notify(nameof(SelectedEventType)); }
        }

        private AttendanceComboItem _selectedAttendance;
        public AttendanceComboItem SelectedAttendance
        {
            get => _selectedAttendance;
            set { _selectedAttendance = value; Notify(nameof(SelectedAttendance)); }
        }

        private string _avgCostText = "0";
        public string AverageCostText
        {
            get => _avgCostText;
            set { _avgCostText = value ?? "0"; Notify(nameof(AverageCostText)); }
        }

        private string _country = "";
        public string Country
        {
            get => _country;
            set { _country = value ?? ""; Notify(nameof(Country)); }
        }

        private string _city = "";
        public string City
        {
            get => _city;
            set { _city = value ?? ""; Notify(nameof(City)); }
        }

        private DateTime? _currentYearDate;
        public DateTime? CurrentYearDate
        {
            get => _currentYearDate;
            set { _currentYearDate = value; Notify(nameof(CurrentYearDate)); }
        }

        private DateTime? _pastDateToAdd;
        public DateTime? PastDateToAdd
        {
            get => _pastDateToAdd;
            set { _pastDateToAdd = value; Notify(nameof(PastDateToAdd)); }
        }

        private bool _isHumanitarian;
        public bool IsHumanitarian
        {
            get => _isHumanitarian;
            set { _isHumanitarian = value; Notify(nameof(IsHumanitarian)); }
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


        private bool _eventIdHasError;
        public bool EventIdHasError
        {
            get => _eventIdHasError;
            set { _eventIdHasError = value; Notify(nameof(EventIdHasError)); }
        }

        private bool _nameHasError;
        public bool NameHasError
        {
            get => _nameHasError;
            set { _nameHasError = value; Notify(nameof(NameHasError)); }
        }

        private bool _eventTypeHasError;
        public bool EventTypeHasError
        {
            get => _eventTypeHasError;
            set { _eventTypeHasError = value; Notify(nameof(EventTypeHasError)); }
        }


        public ObservableCollection<EventTypeComboItem> EventTypeItems { get; }
            = new ObservableCollection<EventTypeComboItem>();

        public List<AttendanceComboItem> AttendanceItems { get; }
            = new List<AttendanceComboItem>
            {
                new AttendanceComboItem { Value = AttendanceRange.Upto1000,        DisplayName = "Up to 1,000"    },
                new AttendanceComboItem { Value = AttendanceRange.From1000To5000,  DisplayName = "1,000 – 5,000"  },
                new AttendanceComboItem { Value = AttendanceRange.From5000To10000, DisplayName = "5,000 – 10,000" },
                new AttendanceComboItem { Value = AttendanceRange.Over10000,       DisplayName = "Over 10,000"    },
            };

        public ObservableCollection<TagCheckItem> Tags { get; }
            = new ObservableCollection<TagCheckItem>();

        public ObservableCollection<HistoryDateItem> HistoryDates { get; }
            = new ObservableCollection<HistoryDateItem>();

        public List<IconComboItem> IconItems { get; }


        public EditEventViewModel(Event ev, List<EventType> eventTypes, List<Tag> tags)
        {
            OriginalId = ev.Id;

            const string pack = "pack://application:,,,/EventsApp;component/";

            EventTypeItems.Add(new EventTypeComboItem { Id = 0, Name = "(Select type)" });
            foreach (var t in eventTypes)
                EventTypeItems.Add(new EventTypeComboItem { Id = t.Id, Name = t.Name });

            var tagIdSet = new HashSet<string>(ev.TagIds ?? new List<string>());
            foreach (var t in tags)
                Tags.Add(new TagCheckItem { Id = t.Id, DisplayName = t.Description, IsSelected = tagIdSet.Contains(t.Id.ToString()) });

            IconItems = new List<IconComboItem>
            {
                new IconComboItem { DisplayName = "(None)",      FilePath = null,                               PreviewPath = null },
                new IconComboItem { DisplayName = "Film",        FilePath = "Resources/Images/film.png",        PreviewPath = pack + "Resources/Images/film.png" },
                new IconComboItem { DisplayName = "Music",       FilePath = "Resources/Images/music.png",       PreviewPath = pack + "Resources/Images/music.png" },
                new IconComboItem { DisplayName = "Tennis",      FilePath = "Resources/Images/tennis.png",      PreviewPath = pack + "Resources/Images/tennis.png" },
                new IconComboItem { DisplayName = "Basketball",  FilePath = "Resources/Images/basketball.png",  PreviewPath = pack + "Resources/Images/basketball.png" },
                new IconComboItem { DisplayName = "Art",         FilePath = "Resources/Images/art.png",         PreviewPath = pack + "Resources/Images/art.png" },
            };

            EventIdText        = ev.Id.ToString();
            Name               = ev.Name ?? "";
            Description        = ev.Description ?? "";
            AverageCostText    = ev.AverageCost.ToString();
            Country            = ev.Country ?? "";
            City               = ev.City ?? "";
            CurrentYearDate    = ev.CurrentYearDate;
            IsHumanitarian     = ev.IsHumanitarian;

            SelectedEventType  = EventTypeItems.FirstOrDefault(i => i.Id == ev.EventTypeId) ?? EventTypeItems[0];
            SelectedAttendance = AttendanceItems.FirstOrDefault(a => a.Value == ev.Attendance) ?? AttendanceItems[0];
            SelectedIcon       = IconItems.FirstOrDefault(i => i.FilePath == ev.IconPath) ?? IconItems[0];

            foreach (var d in (ev.HistoryDates ?? new List<DateTime>()).OrderByDescending(d => d))
                HistoryDates.Add(new HistoryDateItem { Date = d });
        }


        public void AddPastDate()
        {
            if (!PastDateToAdd.HasValue) return;
            var date = PastDateToAdd.Value.Date;
            if (HistoryDates.All(h => h.Date != date))
                HistoryDates.Add(new HistoryDateItem { Date = date });
            PastDateToAdd = null;
        }

        public void RemovePastDate(HistoryDateItem item)
        {
            HistoryDates.Remove(item);
        }
    }
}
