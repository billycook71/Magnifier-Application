using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MagnifierApplication.Services
{
    ///Retrieves the current global cursor position from Windows.
    ///Separated into its own service so the UI and rendering pipeline
    ///remain independent of direct Win32 API calls.
    internal class CursorService
    {
        ///Native Win32 function that retrieves the current position
        ///in screen coordinates.
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        //Returns the current curosor position as a System.Drawing.Point.
        public Point GetPosition()
        {
            GetCursorPos(out POINT p);
            return new Point(p.X, p.Y);
        }
        
        //Native POINT structure used by the Win32 API
        private struct POINT
        {
            public int X;
            public int Y;
        }

    }
}
