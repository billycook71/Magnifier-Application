using MagnifierApplication.Rendering;
using MagnifierApplication.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media.Imaging;

namespace MagnifierApplication.Core
{
    ///coordinates the magnifier pipeline
    ///1. Read cursor position
    ///2. Capture region of the screen
    ///3. Render the zoomed result
    internal class MagnifierEngine
    {
        private readonly CursorService _cursor;
        private readonly ScreenCaptureService _capture;
        private readonly MagnifierRenderer _renderer;
        private readonly Settings _settings;

        public MagnifierEngine(
            CursorService cursor,
            ScreenCaptureService capture,
            MagnifierRenderer renderer,
            Settings settings)
        {
            _cursor = cursor;
            _capture = capture;
            _renderer = renderer;
            _settings = settings;
        }

        //Produces the latest magnified frame based on current cursor position
        public BitmapSource UpdateFrame()
        {
            var cursorPos = _cursor.GetPosition();

            //calculate the capturesize with the magnification spec
            int captureSize = Math.Max(
                1, (int)(_settings.LensSize / _settings.Magnification)
                );

            //Define the area of the screen to capture
            //Capture offset in settings determines what appears inside the magnifier
            var region = new Rectangle(
            (int)cursorPos.X + _settings.CaptureOffsetX,
            (int)cursorPos.Y + _settings.CaptureOffsetY,
            captureSize,
            captureSize
            );

            //Capture the raw screen region, then render at the zoom level from settings
            using var raw = _capture.Capture(region);
            return _renderer.Render(raw, _settings.LensSize);
        }
    }
}
