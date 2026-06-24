using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class EventTypeComboItem
    {
        public int    Id   { get; set; }
        public string Name { get; set; }
    }

    public class AttendanceComboItem
    {
        public AttendanceRange Value       { get; set; }
        public string          DisplayName { get; set; }
        public bool            IsNone      { get; set; }
    }

    public class TagCheckItem : INotifyPropertyChanged
    {
        public int    Id          { get; set; }
        public string DisplayName { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class HistoryDateItem
    {
        public DateTime Date        { get; set; }
        public string   DisplayText => Date.ToString("yyyy-MM-dd");
    }

    public class IconComboItem
    {
        public string DisplayName { get; set; }
        public string FilePath    { get; set; }
        public string PreviewPath { get; set; }
    }

    public class AddEventViewModel : ViewModelBase
    {
        // ── Form fields ──────────────────────────────────────────────────────

        private string _eventIdText = "";
        public string EventIdText
        {
            get => _eventIdText;
            set => SetProperty(ref _eventIdText, value ?? "");
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

        private EventTypeComboItem _selectedEventType;
        public EventTypeComboItem SelectedEventType
        {
            get => _selectedEventType;
            set => SetProperty(ref _selectedEventType, value);
        }

        private AttendanceComboItem _selectedAttendance;
        public AttendanceComboItem SelectedAttendance
        {
            get => _selectedAttendance;
            set => SetProperty(ref _selectedAttendance, value);
        }

        private string _avgCostText = "0";
        public string AverageCostText
        {
            get => _avgCostText;
            set => SetProperty(ref _avgCostText, value ?? "0");
        }

        private string _country = "";
        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value ?? "");
        }

        private string _city = "";
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value ?? "");
        }

        private DateTime? _currentYearDate;
        public DateTime? CurrentYearDate
        {
            get => _currentYearDate;
            set => SetProperty(ref _currentYearDate, value);
        }

        private DateTime? _pastDateToAdd;
        public DateTime? PastDateToAdd
        {
            get => _pastDateToAdd;
            set => SetProperty(ref _pastDateToAdd, value);
        }

        private bool _isHumanitarian;
        public bool IsHumanitarian
        {
            get => _isHumanitarian;
            set => SetProperty(ref _isHumanitarian, value);
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

        public bool EventIdHasError  => GetErrors(nameof(EventIdText)).Cast<string>().Any();
        public bool NameHasError     => GetErrors(nameof(Name)).Cast<string>().Any();
        public bool EventTypeHasError => GetErrors(nameof(SelectedEventType)).Cast<string>().Any();

        public string EventIdError    => GetErrors(nameof(EventIdText)).Cast<string>().FirstOrDefault() ?? "";
        public string NameError       => GetErrors(nameof(Name)).Cast<string>().FirstOrDefault() ?? "";
        public string EventTypeError  => GetErrors(nameof(SelectedEventType)).Cast<string>().FirstOrDefault() ?? "";

        public bool   AttendanceHasError => GetErrors(nameof(SelectedAttendance)).Cast<string>().Any();
        public string AttendanceError    => GetErrors(nameof(SelectedAttendance)).Cast<string>().FirstOrDefault() ?? "";

        public bool   AvgCostHasError    => GetErrors(nameof(AverageCostText)).Cast<string>().Any();
        public string AvgCostError       => GetErrors(nameof(AverageCostText)).Cast<string>().FirstOrDefault() ?? "";

        public void SetEventIdError(string message) => SetErrors(nameof(EventIdText), new[] { message });

        // ── Validation logic ─────────────────────────────────────────────────

        protected override void ValidateProperty(object value, string propertyName)
        {
            switch (propertyName)
            {
                case nameof(EventIdText):
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

                case nameof(SelectedEventType):
                    var et = value as EventTypeComboItem;
                    if (et == null || et.Id == 0)
                        SetErrors(propertyName, new[] { "Required field" });
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(SelectedAttendance):
                    var att = value as AttendanceComboItem;
                    if (att == null || att.IsNone)
                        SetErrors(propertyName, new[] { "Required field" });
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(AverageCostText):
                    var costText = value as string ?? "";
                    if (string.IsNullOrWhiteSpace(costText))
                        ClearErrors(propertyName);
                    else if (!double.TryParse(costText.Trim(), out double cost))
                        SetErrors(propertyName, new[] { "Must be a valid number" });
                    else if (cost < 0)
                        SetErrors(propertyName, new[] { "Must be 0 or greater" });
                    else
                        ClearErrors(propertyName);
                    break;
            }
        }

        // ── Collections ──────────────────────────────────────────────────────

        public ObservableCollection<EventTypeComboItem> EventTypeItems { get; }
            = new ObservableCollection<EventTypeComboItem>();

        public List<AttendanceComboItem> AttendanceItems { get; }
            = new List<AttendanceComboItem>
            {
                new AttendanceComboItem { DisplayName = "(Select attendance)", IsNone = true                                           },
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


        public AddEventViewModel()
        {
            const string pack = "pack://application:,,,/EventsApp;component/";

            var svc = new EventService();

            EventTypeItems.Add(new EventTypeComboItem { Id = 0, Name = "(Select type)" });
            foreach (var t in svc.LoadEventTypes())
                EventTypeItems.Add(new EventTypeComboItem { Id = t.Id, Name = t.Name });
            _selectedEventType = EventTypeItems[0];

            foreach (var t in svc.LoadTags())
                Tags.Add(new TagCheckItem { Id = t.Id, DisplayName = t.Description, IsSelected = false });

            _selectedAttendance = AttendanceItems[0];

            IconItems = new List<IconComboItem>
            {
                new IconComboItem { DisplayName = "(None)",     FilePath = null,                                PreviewPath = null },
                new IconComboItem { DisplayName = "Film",       FilePath = "Resources/Images/film.png",         PreviewPath = pack + "Resources/Images/film.png" },
                new IconComboItem { DisplayName = "Music",      FilePath = "Resources/Images/music.png",        PreviewPath = pack + "Resources/Images/music.png" },
                new IconComboItem { DisplayName = "Tennis",     FilePath = "Resources/Images/tennis.png",       PreviewPath = pack + "Resources/Images/tennis.png" },
                new IconComboItem { DisplayName = "Basketball", FilePath = "Resources/Images/basketball.png",   PreviewPath = pack + "Resources/Images/basketball.png" },
                new IconComboItem { DisplayName = "Art",        FilePath = "Resources/Images/art.png",          PreviewPath = pack + "Resources/Images/art.png" },
            };
            _selectedIcon = IconItems[0];

            ErrorsChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EventIdText))
                {
                    OnPropertyChanged(nameof(EventIdHasError));
                    OnPropertyChanged(nameof(EventIdError));
                }
                else if (e.PropertyName == nameof(Name))
                {
                    OnPropertyChanged(nameof(NameHasError));
                    OnPropertyChanged(nameof(NameError));
                }
                else if (e.PropertyName == nameof(SelectedEventType))
                {
                    OnPropertyChanged(nameof(EventTypeHasError));
                    OnPropertyChanged(nameof(EventTypeError));
                }
                else if (e.PropertyName == nameof(SelectedAttendance))
                {
                    OnPropertyChanged(nameof(AttendanceHasError));
                    OnPropertyChanged(nameof(AttendanceError));
                }
                else if (e.PropertyName == nameof(AverageCostText))
                {
                    OnPropertyChanged(nameof(AvgCostHasError));
                    OnPropertyChanged(nameof(AvgCostError));
                }
            };
        }

        // ── Methods ──────────────────────────────────────────────────────────

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
