using MagnifierApplication.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
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
        public BitmapSource Render(Bitmap src, int targetSize, RenderingMode renderingMode)
        {
            //cretes a larger bitmap based on the zoom multiplier
            using var rendered = new Bitmap(targetSize, targetSize);

            using (Graphics g = Graphics.FromImage(rendered))
            {
                //nearest neighbor keeps edges sharper to avoid blurry text
                //highest quality bicubic prioritizes smoothness, less accurate, more visual clarity
                g.InterpolationMode = renderingMode == RenderingMode.Sharp
                    ? System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor
                    : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                //Draw the source bitmap into the larger destination bitmap
                g.DrawImage(src, 0, 0, targetSize, targetSize);
            }

            return ConvertToBitmapSource(rendered);
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        ///Converts a System.Drawing.Bitmap into a WPF BitmapSource
        ///WPF and GDI+ use diferent image types, requiring conversion
        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            try
            {
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                    );

                source.Freeze();
                return source;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }
    }
}
