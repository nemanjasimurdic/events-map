using System.ComponentModel;

namespace EventsApp.ViewModel
{
    public class SearchEventTypeViewModel : INotifyPropertyChanged
    {
        private string _name = "";
        public string Name
        {
            get => _name;
            set { _name = value ?? ""; Notify(nameof(Name)); }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set { _description = value ?? ""; Notify(nameof(Description)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Reset()
        {
            Name        = "";
            Description = "";
        }

        private void Notify(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
