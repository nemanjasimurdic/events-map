using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class AddEventTypeWindow : Window
    {
        private readonly EventTypesViewModel _eventTypesViewModel;

        public AddEventTypeWindow(EventTypesViewModel eventTypesViewModel)
        {
            InitializeComponent();
            _eventTypesViewModel = eventTypesViewModel;
            DataContext = new AddEventTypeViewModel();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AddEventTypeViewModel)DataContext;

            int id;
            if (!int.TryParse(vm.IdText.Trim(), out id) || id <= 0) return;

            var svc      = new EventService();
            var existing = svc.LoadEventTypes();
            if (existing.Any(t => t.Id == id)) return;

            var newType = new EventType
            {
                Id          = id,
                Name        = vm.Name.Trim(),
                Description = vm.Description.Trim(),
                IconPath    = vm.SelectedIcon?.FilePath
            };

            existing.Add(newType);
            svc.SaveEventTypes(existing);

            const string pack = "pack://application:,,,/EventsApp;component/";
            string rowIconPath = newType.IconPath != null
                ? pack + newType.IconPath
                : pack + "Resources/Images/world-map.png";

            _eventTypesViewModel.AddRow(newType, rowIconPath);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
