using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class AddEventWindow : Window
    {
        private readonly EventsViewModel _eventsViewModel;

        public AddEventWindow(EventsViewModel eventsViewModel)
        {
            InitializeComponent();
            _eventsViewModel = eventsViewModel;
            DataContext = new AddEventViewModel();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AddEventViewModel)DataContext;
            vm.ValidateAllProperties();
            if (vm.HasErrors) return;

            int eventId = int.Parse(vm.EventIdText.Trim());

            var svc            = new EventService();
            var existingEvents = svc.LoadEvents();
            if (existingEvents.Any(ev => ev.Id == eventId))
            {
                vm.SetEventIdError("ID already exists");
                return;
            }

            double avgCost;
            if (!double.TryParse(vm.AverageCostText.Trim(), out avgCost) || avgCost < 0)
                avgCost = 0.0;

            var newEvent = new Event
            {
                Id              = eventId,
                Name            = vm.Name.Trim(),
                Description     = vm.Description.Trim(),
                EventTypeId     = vm.SelectedEventType.Id,
                Attendance      = vm.SelectedAttendance.Value,
                IconPath        = vm.SelectedIcon?.FilePath,
                IsHumanitarian  = vm.IsHumanitarian,
                AverageCost     = avgCost,
                Country         = vm.Country.Trim(),
                City            = vm.City.Trim(),
                CurrentYearDate = vm.CurrentYearDate,
                HistoryDates    = vm.HistoryDates.Select(h => h.Date).ToList(),
                TagIds          = vm.Tags.Where(t => t.IsSelected).Select(t => t.Id.ToString()).ToList(),
                MapX            = null,
                MapY            = null
            };

            existingEvents.Add(newEvent);
            svc.SaveEvents(existingEvents);

            const string pack = "pack://application:,,,/EventsApp;component/";
            string rowIconPath;
            if (newEvent.IconPath != null)
            {
                rowIconPath = pack + newEvent.IconPath;
            }
            else
            {
                var types     = svc.LoadEventTypes();
                var eventType = types.FirstOrDefault(t => t.Id == newEvent.EventTypeId);
                rowIconPath   = eventType?.IconPath != null
                    ? pack + eventType.IconPath
                    : pack + "Resources/Images/world-map.png";
            }

            _eventsViewModel.AddRow(newEvent, vm.SelectedEventType.Name, rowIconPath);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddPastDate_Click(object sender, RoutedEventArgs e)
        {
            ((AddEventViewModel)DataContext).AddPastDate();
        }

        private void RemovePastDate_Click(object sender, RoutedEventArgs e)
        {
            var item = (HistoryDateItem)((Button)sender).DataContext;
            ((AddEventViewModel)DataContext).RemovePastDate(item);
        }
    }
}
