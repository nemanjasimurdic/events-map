using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class SearchEventTypeWindow : Window
    {
        private readonly EventTypesViewModel      _eventTypesViewModel;
        private readonly Action                   _resetFilterBoxCallback;
        private readonly SearchEventTypeViewModel _vm;

        public SearchEventTypeWindow(EventTypesViewModel eventTypesViewModel, SearchEventTypeViewModel searchState, Action resetFilterBoxCallback)
        {
            InitializeComponent();
            _eventTypesViewModel    = eventTypesViewModel;
            _resetFilterBoxCallback = resetFilterBoxCallback;
            _vm                     = searchState;
            DataContext             = _vm;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var svc   = new EventService();
            var types = svc.LoadEventTypes();

            var matchingIds = new HashSet<int>(
                types.Where(et => Matches(et, _vm))
                     .Select(et => et.Id));

            _eventTypesViewModel.ApplySearch(row => matchingIds.Contains(row.Code));
            Close();
        }

        private static bool Matches(EventType et, SearchEventTypeViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.Name) &&
                (et.Name == null || et.Name.IndexOf(vm.Name, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            if (!string.IsNullOrEmpty(vm.Description) &&
                (et.Description == null || et.Description.IndexOf(vm.Description, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            return true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Reset();
            _eventTypesViewModel.ClearSearch();
            _resetFilterBoxCallback();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
