using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MagnifierApplication.Services
{
    ///Responsible for capturing a rectangular region of the screen
    ///This class hides the screen capture implementation from the rest of the app
    internal class ScreenCaptureService
    {
        //Captures a region of the screen and returns it as a Bitmap
        public Bitmap Capture(Rectangle region)
        {
            //Create a bitmap large enough to hold the requested screen region
            Bitmap bmp = new Bitmap(region.Width, region.Height);

            //Draw the screen pixels into the bitmap
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(region.X, region.Y, 0, 0, region.Size);
            }

            return bmp;
        }
    }
}
