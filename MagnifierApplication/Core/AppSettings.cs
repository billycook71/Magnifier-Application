using System;
using System.Collections.Generic;
using System.Text;

namespace MagnifierApplication.Core
{
    public class AppSettings
    {
        public int ActiveProfileIndex { get; set; } = 0;
        public List<ProfileSettings> Profiles { get; set; } = new();

        public static AppSettings CreateDefault()
        {
            return new AppSettings()
            {
                ActiveProfileIndex = 0,
                Profiles =
                {
                    new ProfileSettings("Default", Settings.CreateDefault()),
                    new ProfileSettings("Profile 1", Settings.CreateDefault()),
                    new ProfileSettings("Profile 2", Settings.CreateDefault()),
                    new ProfileSettings("Profile 3", Settings.CreateDefault()),
                    new ProfileSettings("Profile 4", Settings.CreateDefault())
                }
            };
        }

    }
}
