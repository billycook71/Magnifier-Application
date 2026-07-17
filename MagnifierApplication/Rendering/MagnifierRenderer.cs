using MagnifierApplication.Core;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MagnifierApplication.Rendering
{
    ///Scales captured screen images and converts them into WPF-compatible
    ///BitmapSource objects for display in the magnifier lens.
    internal class MagnifierRenderer
    {
        ///Scales the captured bitmap to the configured lens size using
        ///the selected rendering mode
        public BitmapSource Render(Bitmap src, int targetSize, RenderingMode renderingMode)
        {
            //Create the destination bitmap at the configured lens size.
            using var rendered = new Bitmap(targetSize, targetSize);

            using (Graphics g = Graphics.FromImage(rendered))
            {
                //Nearest neighbor preserves sharp pixel and text edges.
                //Bicubic interpolatrion produces a smoother result.
                g.InterpolationMode = renderingMode == RenderingMode.Sharp
                    ? System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor
                    : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                //Scale the captured image into the destination bitmap.
                g.DrawImage(src, 0, 0, targetSize, targetSize);
            }

            return ConvertToBitmapSource(rendered);
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        ///Converts a GDI+ bitmap into a WPF BitmapSource and releases
        ///temporary native bitmap handle after conversion.
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
