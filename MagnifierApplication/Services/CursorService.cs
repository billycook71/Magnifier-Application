using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

///Service for retrieving current global cursor position from Windows
///Separated into its own service so UI and engine code don't directly 
///depend on WinAPI calls.

namespace MagnifierApplication.Services
{
    internal class CursorService
    {
        ///Native Windows API call to retrieve curosor position in screen coordinates
        ///user32.dll is part of Windows API
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        //Returns the current curosor position as a WPF(Windows Presentation Foundation) Point
        public Point GetPosition()
        {
            GetCursorPos(out POINT p);
            return new Point(p.X, p.Y);
        }
        
        //Native POINT struct used by Windows API
        private struct POINT
        {
            public int X;
            public int Y;
        }

    }
}
