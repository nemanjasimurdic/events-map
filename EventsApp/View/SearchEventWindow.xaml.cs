using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class SearchEventWindow : Window
    {
        private readonly EventsViewModel      _eventsViewModel;
        private readonly Action               _resetFilterBoxCallback;
        private readonly SearchEventViewModel _vm;

        public SearchEventWindow(EventsViewModel eventsViewModel, SearchEventViewModel searchState, Action resetFilterBoxCallback)
        {
            InitializeComponent();
            _eventsViewModel        = eventsViewModel;
            _resetFilterBoxCallback = resetFilterBoxCallback;
            _vm                     = searchState;
            DataContext             = _vm;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var svc    = new EventService();
            var events = svc.LoadEvents();

            double? avgCost = double.TryParse(_vm.AvgCostText, out double costVal) ? costVal : (double?)null;

            var matchingIds = new HashSet<int>(
                events.Where(ev => Matches(ev, _vm, avgCost))
                      .Select(ev => ev.Id));

            _eventsViewModel.ApplySearch(row => matchingIds.Contains(row.EventId));
            Close();
        }

        private static bool Matches(Event ev, SearchEventViewModel vm, double? avgCost)
        {
            if (!string.IsNullOrEmpty(vm.Name) &&
                (ev.Name == null || ev.Name.IndexOf(vm.Name, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (vm.SelectedEventType?.Id != null && ev.EventTypeId != vm.SelectedEventType.Id)
                return false;

            if (!string.IsNullOrEmpty(vm.Country) &&
                (ev.Country == null || ev.Country.IndexOf(vm.Country, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (!string.IsNullOrEmpty(vm.City) &&
                (ev.City == null || ev.City.IndexOf(vm.City, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (!string.IsNullOrEmpty(vm.Description) &&
                (ev.Description == null || ev.Description.IndexOf(vm.Description, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (vm.SelectedAttendance?.Value != null && ev.Attendance != vm.SelectedAttendance.Value)
                return false;

            if (avgCost.HasValue && ev.AverageCost != avgCost.Value)
                return false;

            if (vm.IsHumanitarianFilter.HasValue && ev.IsHumanitarian != vm.IsHumanitarianFilter.Value)
                return false;

            return true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Reset();
            _eventsViewModel.ClearSearch();
            _resetFilterBoxCallback();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
