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

            int id;
            if (!int.TryParse(vm.IdText.Trim(), out id) || id <= 0) return;

            var svc      = new EventService();
            var existing = svc.LoadTags();
            if (existing.Any(t => t.Id == id)) return;

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
