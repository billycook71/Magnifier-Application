using System;
using System.Collections.Generic;
using System.Text;


///Used to store values that control how the magnifier behaves.
///Later, these will be adjusted through a GUI window as opposed to being hardcoded.

namespace MagnifierApplication.Core
{
    public enum LensShape
    {
        Circle,
        Square
    }

    public class Settings
    {
        //set default shape to circle
        public LensShape Shape = LensShape.Circle;
        
        //Width/height of the capture window
        public int CaptureSize = 100;

        //How much to enlarge the captured region
        public int Zoom = 3;

        //How much to offset the captured region in relevance to the cursor
        //Affects what the lense shows
        public int CaptureOffsetX = -40;
        public int CaptureOffsetY = -40;

        //Offset the magnifier relevant to the cursor
        //Determines where the lense appears on the screen
        public int WindowOffsetX = 140;
        public int WindowOffsetY = 40;

        //Diameter of the lens
        public int LensSize = 200;
    }
}
