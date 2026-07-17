using MagnifierApplication.Rendering;
using MagnifierApplication.Services;
using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace MagnifierApplication.Core
{
    ///coordinates the magnifier pipeline by reading cursor position,
    ///capturing the configured screen region and rendering the magnified frame.
    internal class MagnifierEngine
    {
        private readonly CursorService _cursor;
        private readonly ScreenCaptureService _capture;
        private readonly MagnifierRenderer _renderer;
        public Settings Settings { get; set; }

        public MagnifierEngine(
            CursorService cursor,
            ScreenCaptureService capture,
            MagnifierRenderer renderer,
            Settings settings)
        {
            _cursor = cursor;
            _capture = capture;
            _renderer = renderer;
            Settings = settings;
        }

        ///Produces the latest magnified frame based on current cursor position
        ///and active profile settings.
        public BitmapSource UpdateFrame()
        {
            var cursorPos = _cursor.GetPosition();

            //Calculate the source region size needed for the configured zoom level
            int captureSize = Math.Max(
                1, (int)(Settings.LensSize / Settings.Magnification)
                );

            //Build the screen region relative to the cursor and capture offsets
            var region = new Rectangle(
            (int)cursorPos.X + Settings.CaptureOffsetX,
            (int)cursorPos.Y + Settings.CaptureOffsetY,
            captureSize,
            captureSize
            );

            //Capture the source region and scale it to the configured lens size
            using var raw = _capture.Capture(region);
            return _renderer.Render(raw, Settings.LensSize, Settings.RenderingMode);
        }
    }
}
