using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MagnifierApplication.Rendering
{
    ///Responsible for taking a captured bitmap and enlarging it,
    ///converting it into a WPF-compatible image source
    internal class MagnifierRenderer
    {
        ///Enlarges source image and returns as a bitmap source to be displayed by WPF Image control
        public BitmapSource Render(Bitmap src, int targetSize)
        {
            //cretes a larger bitmap based on the zoom multiplier
            var rendered = new Bitmap(targetSize, targetSize);

            using (Graphics g = Graphics.FromImage(rendered))
            {
                //nearest neighbor keeps edges sharper to avoid blurry text
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                //Draw the source bitmap into the larger destination bitmap
                g.DrawImage(src, 0, 0, targetSize, targetSize);
            }

            return ConvertToBitmapSource(rendered);
        }

        ///Converts a System.Drawing.Bitmap into a WPF BitmapSource
        ///WPF and GDI+ use diferent image types, requiring conversion
        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();

            return Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
                );
        }
    }
}
