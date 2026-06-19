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
        public LensShape Shape { get; set; } = LensShape.Circle;

        //Magnification of the lens (calculated with capturesize in engine)
        public double Magnification { get; set; } = 2.0;

        //How much to offset the captured region in relevance to the cursor
        //Affects what the lense shows
        public int CaptureOffsetX { get; set; } = -40;
        public int CaptureOffsetY { get; set; } = -40;

        //Offset the magnifier relevant to the cursor
        //Determines where the lense appears on the screen
        public int WindowOffsetX { get; set; } = 140;
        public int WindowOffsetY { get; set; } = 40;

        //Diameter of the lens
        public int LensSize { get; set; } = 200;

        //Thickness of lens border
        public int BorderThickness { get; set; } = 3;
    }
}
