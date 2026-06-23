using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventsApp.Services;
using EventsApp.View;
using EventsApp.ViewModel;

namespace EventsApp.Pages
{
    public partial class EventTypesPage : Page
    {
        public EventTypesPage()
        {
            InitializeComponent();
            DataContext = new EventTypesViewModel();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (EventTypeRowItem)btn.DataContext;

            var svc       = new EventService();
            var eventType = svc.LoadEventTypes().FirstOrDefault(t => t.Id == rowItem.Code);
            if (eventType == null) return;

            var dlg = new EventTypeDetailWindow(new EventTypeDetailViewModel(eventType));
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var btn     = (Button)sender;
            var rowItem = (EventTypeRowItem)btn.DataContext;

            var dlg = new DeleteConfirmWindow("Are you sure you want to delete this event type?");
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;

            var svc   = new EventService();
            var types = svc.LoadEventTypes();
            types.RemoveAll(t => t.Id == rowItem.Code);
            svc.SaveEventTypes(types);

            ((EventTypesViewModel)DataContext).EventTypes.Remove(rowItem);
        }
    }
}
