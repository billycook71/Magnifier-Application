using System;
using System.Drawing;
using System.ComponentModel;

namespace MagnifierApplication.Services
{
    ///Captures a rectangular region of the virtual desktop while clamping
    ///requested coordinates and dimensions to valid screen bounds.
    internal class ScreenCaptureService
    {
        ///Captures a region of the screen and returns it as a Bitmap.
        ///Returns a black fallback frame if the Windows capture operation
        ///fails temporarily.
        public Bitmap Capture(Rectangle region)
        {
            Rectangle screenBounds = System.Windows.Forms.SystemInformation.VirtualScreen;

            int width = Math.Max(1, region.Width);
            int height = Math.Max(1, region.Height);


            //Ensure the capture dimensions do not exceed the virtual desktop.
            if (width > screenBounds.Width)
                width = screenBounds.Width;

            if (height > screenBounds.Height)
                height = screenBounds.Height;

            int x = region.X;
            int y = region.Y;

            //Clamp the capture origin so the full region remains on-screen.
            if (x < screenBounds.Left)
                x = screenBounds.Left;

            if (y < screenBounds.Top)
                y = screenBounds.Top;

            if (x + width > screenBounds.Right)
                x = screenBounds.Right - width;

            if (y + height > screenBounds.Bottom)
                y = screenBounds.Bottom - height;


            Bitmap bmp = new Bitmap(width, height);

            try
            {
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

        //Creates a safe frame so a transient capture failure does not crash teh app.
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
