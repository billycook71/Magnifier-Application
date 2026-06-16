using MagnifierApplication.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MagnifierApplication
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Settings _settings;
        public SettingsWindow(Settings settings)
        {
            InitializeComponent();

            _settings = settings;

            LoadSettingsIntoControls();
            WireEvents();
        }

        private void LoadSettingsIntoControls()
        {
            ZoomSlider.Value = _settings.Zoom;
            CaptureSizeSlider.Value = _settings.CaptureSize;
            LensSizeSlider.Value = _settings.LensSize;
            WindowOffsetXSlider.Value = _settings.WindowOffsetX;
            WindowOffsetYSlider.Value = _settings.WindowOffsetY;

            ShapeComboBox.SelectedIndex=
                _settings.Shape == LensShape.Circle ? 0 : 1;
        }

        private void WireEvents()
        {
            ZoomSlider.ValueChanged += (s, e) =>
                _settings.Zoom = (int)ZoomSlider.Value;

            CaptureSizeSlider.ValueChanged += (s, e) =>
                _settings.CaptureSize = (int)CaptureSizeSlider.Value;

            LensSizeSlider.ValueChanged += (s, e) =>
                _settings.LensSize = (int)LensSizeSlider.Value;

            WindowOffsetXSlider.ValueChanged += (s, e) =>
                _settings.WindowOffsetX = (int)WindowOffsetXSlider.Value;

            WindowOffsetYSlider.ValueChanged += (s, e) =>
                _settings.WindowOffsetY = (int)WindowOffsetYSlider.Value;


            ShapeComboBox.SelectionChanged += (s, e) =>
            {
                if (ShapeComboBox.SelectedIndex == 0)
                    _settings.Shape = LensShape.Circle;
                else
                    _settings.Shape = LensShape.Square;
            };
        }
    }
}
