using System.Windows.Controls;
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
    }
}
