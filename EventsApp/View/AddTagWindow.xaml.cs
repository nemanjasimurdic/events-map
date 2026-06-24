using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class AddTagWindow : Window
    {
        private readonly TagsViewModel _tagsViewModel;

        public AddTagWindow(TagsViewModel tagsViewModel)
        {
            InitializeComponent();
            _tagsViewModel = tagsViewModel;
            DataContext = new AddTagViewModel();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (AddTagViewModel)DataContext;
            vm.ValidateAllProperties();
            if (vm.HasErrors) return;

            int id = int.Parse(vm.IdText.Trim());

            var svc      = new EventService();
            var existing = svc.LoadTags();
            if (existing.Any(t => t.Id == id))
            {
                vm.SetIdError("ID already exists");
                return;
            }

            var newTag = new Tag
            {
                Id          = id,
                Description = vm.Description.Trim(),
                ColorHex    = vm.SelectedColor?.Hex ?? "#4CAF50"
            };

            existing.Add(newTag);
            svc.SaveTags(existing);

            _tagsViewModel.AddRow(newTag);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
