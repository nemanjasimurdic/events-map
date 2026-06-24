using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventsApp.Services;
using EventsApp.View;
using EventsApp.ViewModel;

namespace EventsApp.Pages
{
    public partial class TagsPage : Page
    {
        public TagsPage()
        {
            InitializeComponent();
            DataContext = new TagsViewModel();
            FilterBox.TextChanged += (s, e) =>
                ((TagsViewModel)DataContext).FilterText = FilterBox.Text;
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ((TagsViewModel)DataContext).FilterText = "";
            FilterBox.Text = "";
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var vm  = (TagsViewModel)DataContext;
            var dlg = new AddTagWindow(vm);
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (TagRowItem)btn.DataContext;

            var vm  = (TagsViewModel)DataContext;
            var dlg = new EditTagWindow(vm, rowItem.Code);
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (TagRowItem)btn.DataContext;

            var svc = new EventService();
            var tag = svc.LoadTags().FirstOrDefault(t => t.Id == rowItem.Code);
            if (tag == null) return;

            var dlg = new TagDetailWindow(new TagDetailViewModel(tag));
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (TagRowItem)btn.DataContext;

            var dlg = new DeleteConfirmWindow("Are you sure you want to delete this tag?");
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;

            var svc  = new EventService();
            var tags = svc.LoadTags();
            tags.RemoveAll(t => t.Id == rowItem.Code);
            svc.SaveTags(tags);

            ((TagsViewModel)DataContext).Tags.Remove(rowItem);
        }
    }
}
