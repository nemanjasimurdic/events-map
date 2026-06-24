using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using EventsApp.Models;
using EventsApp.Services;

namespace EventsApp.ViewModel
{
    public class SearchAttendanceItem
    {
        public AttendanceRange? Value       { get; set; }
        public string           DisplayName { get; set; }
    }

    public class SearchEventTypeItem
    {
        public int?   Id   { get; set; }
        public string Name { get; set; }
    }

    public class SearchEventViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SearchEventTypeItem>  EventTypeItems   { get; }
            = new ObservableCollection<SearchEventTypeItem>();
        public ObservableCollection<SearchAttendanceItem> AttendanceItems  { get; }
            = new ObservableCollection<SearchAttendanceItem>();

        private SearchEventTypeItem _selectedEventType;
        public SearchEventTypeItem SelectedEventType
        {
            get => _selectedEventType;
            set { _selectedEventType = value; Notify(nameof(SelectedEventType)); }
        }

        private SearchAttendanceItem _selectedAttendance;
        public SearchAttendanceItem SelectedAttendance
        {
            get => _selectedAttendance;
            set { _selectedAttendance = value; Notify(nameof(SelectedAttendance)); }
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value ?? ""; Notify(nameof(Name)); }
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

        private string _description = "";
        public string Description
        {
            get => _description;
            set { _description = value ?? ""; Notify(nameof(Description)); }
        }

        private string _avgCostText = "";
        public string AvgCostText
        {
            get => _avgCostText;
            set { _avgCostText = value ?? ""; Notify(nameof(AvgCostText)); }
        }

        private bool? _isHumanitarianFilter;
        public bool? IsHumanitarianFilter
        {
            get => _isHumanitarianFilter;
            set { _isHumanitarianFilter = value; Notify(nameof(IsHumanitarianFilter)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SearchEventViewModel(List<EventType> eventTypes)
        {
            EventTypeItems.Add(new SearchEventTypeItem { Id = null, Name = "(Any)" });
            foreach (var et in eventTypes)
                EventTypeItems.Add(new SearchEventTypeItem { Id = et.Id, Name = et.Name });

            AttendanceItems.Add(new SearchAttendanceItem { Value = null,                              DisplayName = "(Any)"          });
            AttendanceItems.Add(new SearchAttendanceItem { Value = AttendanceRange.Upto1000,          DisplayName = "Up to 1,000"    });
            AttendanceItems.Add(new SearchAttendanceItem { Value = AttendanceRange.From1000To5000,    DisplayName = "1,000 – 5,000"  });
            AttendanceItems.Add(new SearchAttendanceItem { Value = AttendanceRange.From5000To10000,   DisplayName = "5,000 – 10,000" });
            AttendanceItems.Add(new SearchAttendanceItem { Value = AttendanceRange.Over10000,         DisplayName = "Over 10,000"    });

            Reset();
        }

        public void Reset()
        {
            Name                 = "";
            Country              = "";
            City                 = "";
            Description          = "";
            AvgCostText          = "";
            IsHumanitarianFilter = null;
            SelectedEventType    = EventTypeItems[0];
            SelectedAttendance   = AttendanceItems[0];
        }

        private void Notify(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
