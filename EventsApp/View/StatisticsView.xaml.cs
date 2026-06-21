using System.Windows.Controls;
using EventsApp.ViewModel;

namespace EventsApp.Pages
{
    public partial class StatisticsPage : Page
    {
        public StatisticsPage()
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel();
        }
    }
}
