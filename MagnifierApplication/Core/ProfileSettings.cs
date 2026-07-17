using System;
using System.Collections.Generic;
using System.Text;

namespace MagnifierApplication.Core
{
    //Represents a named magnifier profile and its associated settings.
    public class ProfileSettings
    {
        public string DisplayName { get; set; }
        public Settings Settings { get; set; }

        public ProfileSettings(string displayName, Settings settings)
        {
            DisplayName = displayName;
            Settings = settings;
        }
    }
}
