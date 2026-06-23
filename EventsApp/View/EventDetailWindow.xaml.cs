using System.Windows;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class EventDetailWindow : Window
    {
        public EventDetailWindow(EventDetailViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
