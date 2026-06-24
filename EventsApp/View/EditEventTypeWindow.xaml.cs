using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class EditEventTypeWindow : Window
    {
        private readonly EventTypesViewModel _eventTypesViewModel;

        public EditEventTypeWindow(EventTypesViewModel eventTypesViewModel, int eventTypeId)
        {
            InitializeComponent();
            _eventTypesViewModel = eventTypesViewModel;

            var svc = new EventService();
            var et  = svc.LoadEventTypes().First(t => t.Id == eventTypeId);
            DataContext = new EditEventTypeViewModel(et);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (EditEventTypeViewModel)DataContext;

            int id;
            if (!int.TryParse(vm.IdText.Trim(), out id) || id <= 0) return;

            var svc      = new EventService();
            var existing = svc.LoadEventTypes();
            // ID is valid if not taken by a DIFFERENT event type
            if (existing.Any(t => t.Id == id && t.Id != vm.OriginalId)) return;

            var updatedType = new EventType
            {
                Id          = id,
                Name        = vm.Name.Trim(),
                Description = vm.Description.Trim(),
                IconPath    = vm.SelectedIcon?.FilePath
            };

            int idx = existing.FindIndex(t => t.Id == vm.OriginalId);
            if (idx >= 0)
                existing[idx] = updatedType;
            svc.SaveEventTypes(existing);

            const string pack = "pack://application:,,,/EventsApp;component/";
            string rowIconPath = updatedType.IconPath != null
                ? pack + updatedType.IconPath
                : pack + "Resources/Images/world-map.png";

            _eventTypesViewModel.UpdateRow(vm.OriginalId, updatedType, rowIconPath);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
