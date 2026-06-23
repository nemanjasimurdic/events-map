using EventsApp.Models;

namespace EventsApp.ViewModel
{
    public class EventTypeDetailViewModel
    {
        public string IconPath    { get; }
        public int    Code        { get; }
        public string Name        { get; }
        public string Description { get; }

        public EventTypeDetailViewModel(EventType eventType)
        {
            const string pack = "pack://application:,,,/EventsApp;component/";

            IconPath    = eventType.IconPath != null
                ? pack + eventType.IconPath
                : pack + "Resources/Images/event-type.png";
            Code        = eventType.Id;
            Name        = eventType.Name;
            Description = eventType.Description;
        }
    }
}
