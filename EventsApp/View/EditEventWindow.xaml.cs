using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class EditEventWindow : Window
    {
        private readonly EventsViewModel _eventsViewModel;
        private readonly double? _originalMapX;
        private readonly double? _originalMapY;

        public EditEventWindow(EventsViewModel eventsViewModel, int eventId)
        {
            InitializeComponent();
            _eventsViewModel = eventsViewModel;

            var svc    = new EventService();
            var ev     = svc.LoadEvents().First(e => e.Id == eventId);
            _originalMapX = ev.MapX;
            _originalMapY = ev.MapY;

            DataContext = new EditEventViewModel(ev, svc.LoadEventTypes(), svc.LoadTags());
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var vm    = (EditEventViewModel)DataContext;
            bool valid = true;

            int eventId;
            bool idParsed = int.TryParse(vm.EventIdText.Trim(), out eventId) && eventId > 0;

            var svc            = new EventService();
            var existingEvents = svc.LoadEvents();
            bool idUnique = idParsed && !existingEvents.Any(ev => ev.Id == eventId && ev.Id != vm.OriginalId);

            vm.EventIdHasError = !idParsed || !idUnique;
            if (vm.EventIdHasError) valid = false;

            vm.NameHasError = string.IsNullOrWhiteSpace(vm.Name);
            if (vm.NameHasError) valid = false;

            vm.EventTypeHasError = vm.SelectedEventType == null || vm.SelectedEventType.Id == 0;
            if (vm.EventTypeHasError) valid = false;

            if (!valid) return;

            double avgCost;
            if (!double.TryParse(vm.AverageCostText.Trim(), out avgCost) || avgCost < 0)
                avgCost = 0.0;

            var updatedEvent = new Event
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
                MapX            = _originalMapX,
                MapY            = _originalMapY
            };

            int idx = existingEvents.FindIndex(ev => ev.Id == vm.OriginalId);
            if (idx >= 0)
                existingEvents[idx] = updatedEvent;
            svc.SaveEvents(existingEvents);

            const string pack = "pack://application:,,,/EventsApp;component/";
            string rowIconPath;
            if (updatedEvent.IconPath != null)
            {
                rowIconPath = pack + updatedEvent.IconPath;
            }
            else
            {
                var types     = svc.LoadEventTypes();
                var eventType = types.FirstOrDefault(t => t.Id == updatedEvent.EventTypeId);
                rowIconPath   = eventType?.IconPath != null
                    ? pack + eventType.IconPath
                    : pack + "Resources/Images/world-map.png";
            }

            _eventsViewModel.UpdateRow(vm.OriginalId, updatedEvent, vm.SelectedEventType.Name, rowIconPath);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddPastDate_Click(object sender, RoutedEventArgs e)
        {
            ((EditEventViewModel)DataContext).AddPastDate();
        }

        private void RemovePastDate_Click(object sender, RoutedEventArgs e)
        {
            var item = (HistoryDateItem)((Button)sender).DataContext;
            ((EditEventViewModel)DataContext).RemovePastDate(item);
        }
    }
}
