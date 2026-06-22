using System.Windows.Controls;
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
    }
}
