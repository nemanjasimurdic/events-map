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
            FilterBox.TextChanged += (s, e) =>
                ((EventsViewModel)DataContext).FilterText = FilterBox.Text;
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ((EventsViewModel)DataContext).FilterText = "";
            FilterBox.Text = "";
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (EventRowItem)btn.DataContext;

            var dlg = new DeleteConfirmWindow("Are you sure you want to delete this event?");
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;

            var svc    = new EventService();
            var events = svc.LoadEvents();
            events.RemoveAll(ev => ev.Id == rowItem.EventId);
            svc.SaveEvents(events);

            ((EventsViewModel)DataContext).Events.Remove(rowItem);
        }
    }
}
