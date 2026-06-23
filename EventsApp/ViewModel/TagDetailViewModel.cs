using System.Collections.Generic;
using EventsApp.Models;

namespace EventsApp.ViewModel
{
    public class TagDetailViewModel
    {
        private static readonly Dictionary<string, string> ColorNames =
            new Dictionary<string, string>
            {
                { "#4CAF50", "Green"  },
                { "#2196F3", "Blue"   },
                { "#FF9800", "Amber"  },
                { "#F44336", "Red"    },
                { "#9C27B0", "Purple" }
            };

        public int    Code         { get; }
        public string Description  { get; }
        public string ColorDisplay { get; }

        public TagDetailViewModel(Tag tag)
        {
            Code        = tag.Id;
            Description = tag.Description;

            string colorName;
            if (!ColorNames.TryGetValue(tag.ColorHex, out colorName))
                colorName = tag.ColorHex;
            ColorDisplay = string.Format("{0} ({1})", colorName, tag.ColorHex);
        }
    }
}
