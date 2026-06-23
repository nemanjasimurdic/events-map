using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventsApp.Services;
using EventsApp.View;
using EventsApp.ViewModel;

namespace EventsApp.Pages
{
    public partial class EventsPage : Page
    {
        public EventsPage()
        {
            InitializeComponent();
            DataContext = new EventsViewModel();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (EventRowItem)btn.DataContext;

            var svc    = new EventService();
            var events = svc.LoadEvents();
            var ev     = events.FirstOrDefault(x => x.Id == rowItem.EventId);
            if (ev == null) return;

            var dlg = new EventDetailWindow(
                new EventDetailViewModel(ev, svc.LoadEventTypes(), svc.LoadTags()));
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }
    }
}
