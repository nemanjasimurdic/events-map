using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class EditTagWindow : Window
    {
        private readonly TagsViewModel _tagsViewModel;

        public EditTagWindow(TagsViewModel tagsViewModel, int tagId)
        {
            InitializeComponent();
            _tagsViewModel = tagsViewModel;

            var svc = new EventService();
            var tag = svc.LoadTags().First(t => t.Id == tagId);
            DataContext = new EditTagViewModel(tag);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (EditTagViewModel)DataContext;

            int id;
            if (!int.TryParse(vm.IdText.Trim(), out id) || id <= 0) return;

            var svc      = new EventService();
            var existing = svc.LoadTags();
            // ID is valid if not taken by a DIFFERENT tag
            if (existing.Any(t => t.Id == id && t.Id != vm.OriginalId)) return;

            var updatedTag = new Tag
            {
                Id          = id,
                Description = vm.Description.Trim(),
                ColorHex    = vm.SelectedColor?.Hex ?? "#4CAF50"
            };

            int idx = existing.FindIndex(t => t.Id == vm.OriginalId);
            if (idx >= 0)
                existing[idx] = updatedTag;
            svc.SaveTags(existing);

            _tagsViewModel.UpdateRow(vm.OriginalId, updatedTag);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
