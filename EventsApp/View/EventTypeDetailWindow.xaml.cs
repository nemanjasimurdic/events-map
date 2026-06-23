using System.Windows;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class EventTypeDetailWindow : Window
    {
        public EventTypeDetailWindow(EventTypeDetailViewModel vm)
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
