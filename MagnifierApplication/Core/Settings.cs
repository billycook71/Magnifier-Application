using System;
using System.Collections.Generic;
using System.Text;


///Defines the per-profile settings that control the magnifier's
///appearance, positioning, and rendering behavior.

namespace MagnifierApplication.Core
{
    public enum LensShape
    {
        Circle,
        Square
    }

    public enum RenderingMode
    {
        Sharp,
        Smooth
    }

    public class Settings
    {   
    //Appearance

        //Shape of magnifier lens.
        public LensShape Shape { get; set; } = LensShape.Circle;

        //Diameter of the magnifier lens in pixels.
        public int LensSize { get; set; } = 200;

        //Thickness of lens border in pixels.
        public int BorderThickness { get; set; } = 3;


    //Rendering

        //Rendering mode used when scaling the captured image.
        public RenderingMode RenderingMode { get; set; } = RenderingMode.Sharp;

        //Zoom level applied ot the captured screen region.
        public double Magnification { get; set; } = 2.0;


    //Capture

        //Offsets the captured screen region relative to the cursor
        public int CaptureOffsetX { get; set; } = -40;
        public int CaptureOffsetY { get; set; } = -40;

        //Selected capture preset. Custom is represented by index 5.
        public int CapturePresetIndex { get; set; } = 5;


    //Window

        //Position the magnifier window relative to the cursor
        public int WindowOffsetX { get; set; } = 140;
        public int WindowOffsetY { get; set; } = 40;




        //Creates a new Settings instance using the default application values.
        public static Settings CreateDefault()
        {
            return new Settings();
        }
    }
}
