using System.Windows.Controls;
using EventsApp.ViewModel;

namespace EventsApp.Pages
{
    public partial class TagsPage : Page
    {
        public TagsPage()
        {
            InitializeComponent();
            DataContext = new TagsViewModel();
        }
    }
}
