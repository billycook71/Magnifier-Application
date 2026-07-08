using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.ComponentModel;

namespace MagnifierApplication.Services
{
    ///Responsible for capturing a rectangular region of the screen
    ///This class handles the screen capture implementation for the rest of the app
    ///Also prevents bad values i.e. out of bounds values/near screen edges
    internal class ScreenCaptureService
    {
        //Captures a region of the screen and returns it as a Bitmap
        public Bitmap Capture(Rectangle region)
        {
            Rectangle screenBounds = System.Windows.Forms.SystemInformation.VirtualScreen;

            int width = Math.Max(1, region.Width);
            int height = Math.Max(1, region.Height);


            //if value out of bounds, use max value instead
            if (width > screenBounds.Width)
                width = screenBounds.Width;

            if (height > screenBounds.Height)
                height = screenBounds.Height;

            int x = region.X;
            int y = region.Y;

            if (x < screenBounds.Left)
                x = screenBounds.Left;

            if (y < screenBounds.Top)
                y = screenBounds.Top;

            if (x + width > screenBounds.Right)
                x = screenBounds.Right - width;

            if (y + height > screenBounds.Bottom)
                y = screenBounds.Bottom - height;


            //Create a bitmap large enough to hold the requested screen region
            Bitmap bmp = new Bitmap(width, height);

            try
            {
                //Draw the screen pixels into the bitmap
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
                }

                return bmp;
            }
            catch (Win32Exception)
            {
                bmp.Dispose();
                return CreateFallbackBitmap(width, height);
            }
        }

        private Bitmap CreateFallbackBitmap(int width, int height)
        {
            Bitmap fallback = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(fallback))
            {
                g.Clear(Color.Black);
            }

            return fallback;
        }
    }
}
