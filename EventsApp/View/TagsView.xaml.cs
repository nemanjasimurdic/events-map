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
    }
}
