using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EventsApp.Models;
using EventsApp.Services;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class SearchTagWindow : Window
    {
        private readonly TagsViewModel      _tagsViewModel;
        private readonly Action             _resetFilterBoxCallback;
        private readonly SearchTagViewModel _vm;

        public SearchTagWindow(TagsViewModel tagsViewModel, SearchTagViewModel searchState, Action resetFilterBoxCallback)
        {
            InitializeComponent();
            _tagsViewModel          = tagsViewModel;
            _resetFilterBoxCallback = resetFilterBoxCallback;
            _vm                     = searchState;
            DataContext             = _vm;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var svc  = new EventService();
            var tags = svc.LoadTags();

            var matchingIds = new HashSet<int>(
                tags.Where(t => Matches(t, _vm))
                    .Select(t => t.Id));

            _tagsViewModel.ApplySearch(row => matchingIds.Contains(row.Code));
            Close();
        }

        private static bool Matches(Tag tag, SearchTagViewModel vm)
        {
            if (vm.SelectedColor?.Hex != null && tag.ColorHex != vm.SelectedColor.Hex)
                return false;

            if (!string.IsNullOrEmpty(vm.Description) &&
                (tag.Description == null || tag.Description.IndexOf(vm.Description, StringComparison.OrdinalIgnoreCase) < 0))
                return false;

            return true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Reset();
            _tagsViewModel.ClearSearch();
            _resetFilterBoxCallback();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
