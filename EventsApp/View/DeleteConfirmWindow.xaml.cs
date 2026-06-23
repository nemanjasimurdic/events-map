using System.Windows;

namespace EventsApp.View
{
    public partial class DeleteConfirmWindow : Window
    {
        public DeleteConfirmWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
