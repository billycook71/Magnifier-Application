using System;
using System.Collections.Generic;
using System.Text;


///Used to store values that control how the magnifier behaves.
///Later, these will be adjusted through a GUI window as opposed to being hardcoded.

namespace MagnifierApplication.Core
{
    internal class Settings
    {
        //Width/height of the capture window
        public int CaptureSize = 100;

        //How much to enlarge the captured region
        public int Zoom = 3;

        //How much to offset the captured region in relevance to the cursor
        //Affects what the lense shows
        public int CaptureOffsetX = 0;
        public int CaptureOffsetY = 0;

        //Offset the magnifier relevant to the cursor
        //Determines where the lense appears on the screen
        public int WindowOffsetX = 140;
        public int WindowOffsetY = 40;

        //Diameter of the lense
        public int LensSize = 200;
    }
}
