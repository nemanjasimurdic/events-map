using System.Windows;
using EventsApp.ViewModel;

namespace EventsApp.View
{
    public partial class TagDetailWindow : Window
    {
        public TagDetailWindow(TagDetailViewModel vm)
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
