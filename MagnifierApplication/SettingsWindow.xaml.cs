using MagnifierApplication.Core;
using MagnifierApplication.Services;
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
        private bool _isUpdatingControls;
        private Settings _settings;
        private readonly AppSettings _appSettings;
        private readonly SettingsStorageService _settingsStorage;
        public SettingsWindow(AppSettings appSettings, SettingsStorageService settingsStorage)
        {
            InitializeComponent();

            _appSettings = appSettings;
            _settings = _appSettings.Profiles[_appSettings.ActiveProfileIndex].Settings;
            _settingsStorage = settingsStorage;

            RefreshProfileComboBox();
            LoadSettingsIntoControls();
            UpdateValueLabels();
            WireEvents();
        }

        private void LoadSettingsIntoControls()
        {
            _isUpdatingControls = true;

            MagnificationSlider.Value = _settings.Magnification;
            LensSizeSlider.Value = _settings.LensSize;
            WindowOffsetXSlider.Value = _settings.WindowOffsetX;
            WindowOffsetYSlider.Value = _settings.WindowOffsetY;
            BorderThicknessSlider.Value = _settings.BorderThickness;
            CaptureOffsetXSlider.Value = _settings.CaptureOffsetX;
            CaptureOffsetYSlider.Value = _settings.CaptureOffsetY;
            CapturePresetComboBox.SelectedIndex = 5; //Custom for now

            ProfileComboBox.SelectedIndex =
                _appSettings.ActiveProfileIndex;

            ShapeComboBox.SelectedIndex=
                _settings.Shape == LensShape.Circle ? 0 : 1;

            SharpnessComboBox.SelectedIndex =
                _settings.RenderingMode == RenderingMode.Sharp ? 0 : 1;

            _isUpdatingControls = false;
        }

        private void WireEvents()
        {
            RenameProfileButton.Click += (s, e) =>
            {
                ProfileNameTextBox.Text =
                _appSettings.Profiles[_appSettings.ActiveProfileIndex].DisplayName;

                RenameProfilePanel.Visibility = Visibility.Visible;
                ProfileNameTextBox.Focus();
                ProfileNameTextBox.SelectAll();
            };

            CancelProfileNameButton.Click += (s, e) =>
            {
                RenameProfilePanel.Visibility = Visibility.Collapsed;
            };

            SaveProfileNameButton.Click += (s, e) =>
            {
                string newName = ProfileNameTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                _appSettings.Profiles[_appSettings.ActiveProfileIndex].DisplayName = newName;

                RefreshProfileComboBox();

                RenameProfilePanel.Visibility = Visibility.Collapsed;

                _settingsStorage.Save(_appSettings);
            };


            MagnificationSlider.ValueChanged += (s, e) =>
            {
                _settings.Magnification = MagnificationSlider.Value;
                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };


            LensSizeSlider.ValueChanged += (s, e) =>
            {
                _settings.LensSize = (int)LensSizeSlider.Value;
                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };


            WindowOffsetXSlider.ValueChanged += (s, e) =>
            {
                _settings.WindowOffsetX = (int)WindowOffsetXSlider.Value;
                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };


            WindowOffsetYSlider.ValueChanged += (s, e) =>
            {
                _settings.WindowOffsetY = (int)WindowOffsetYSlider.Value;
                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };
                


            ShapeComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                if (ShapeComboBox.SelectedIndex == 0)
                    _settings.Shape = LensShape.Circle;
                    
                else
                    _settings.Shape = LensShape.Square;

                _settingsStorage.Save(_appSettings);
            };

            SharpnessComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                if (SharpnessComboBox.SelectedIndex == 0)
                    _settings.RenderingMode = RenderingMode.Sharp;

                else
                    _settings.RenderingMode = RenderingMode.Smooth;

                _settingsStorage.Save(_appSettings);
            };

            ProfileComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                int selectedIndex = ProfileComboBox.SelectedIndex;

                if (selectedIndex < 0 || selectedIndex >= _appSettings.Profiles.Count)
                    return;

                _appSettings.ActiveProfileIndex = selectedIndex;
                _settings = _appSettings.Profiles[selectedIndex].Settings;

                LoadSettingsIntoControls();
                UpdateValueLabels();

                ActiveProfileChanged?.Invoke(_settings);

                _settingsStorage.Save(_appSettings);
            };

            CapturePresetComboBox.SelectionChanged += (s, e) =>
            {
                switch (CapturePresetComboBox.SelectedIndex)
                {
                    case 0: //centered
                        _settings.CaptureOffsetX = -50;
                        _settings.CaptureOffsetY = -50;
                        break;

                    case 1: //Above
                        _settings.CaptureOffsetX = -50;
                        _settings.CaptureOffsetY = -100;
                        break;

                    case 2: //Below
                        _settings.CaptureOffsetX = -50;
                        _settings.CaptureOffsetY = 0;
                        break;

                    case 3: //Left
                        _settings.CaptureOffsetX = -100;
                        _settings.CaptureOffsetY = -50;
                        break;

                    case 4: //Right
                        _settings.CaptureOffsetX = 0;
                        _settings.CaptureOffsetY = -50;
                        break;

                    case 5: //Custom
                        return;
                }

                _isUpdatingControls = true;

                CaptureOffsetXSlider.Value = _settings.CaptureOffsetX;
                CaptureOffsetYSlider.Value = _settings.CaptureOffsetY;

                _isUpdatingControls = false;

                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };

            CaptureOffsetXSlider.ValueChanged += (s, e) =>
            {
                _settings.CaptureOffsetX = (int)CaptureOffsetXSlider.Value;

                if (!_isUpdatingControls)
                    CapturePresetComboBox.SelectedIndex = 5;

                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };

            CaptureOffsetYSlider.ValueChanged += (s, e) =>
            {
                _settings.CaptureOffsetY = (int)CaptureOffsetYSlider.Value;

                if (!_isUpdatingControls)
                    CapturePresetComboBox.SelectedIndex = 5;

                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);
            };

            BorderThicknessSlider.ValueChanged += (s, e) =>
            {
                _settings.BorderThickness = (int)BorderThicknessSlider.Value;
                UpdateValueLabels();
                _settingsStorage.Save(_appSettings);

            };

        }

        private void UpdateValueLabels()
        {
            MagnificationValueText.Text = $"{_settings.Magnification:0.0}x";
            LensSizeValueText.Text = $"{_settings.LensSize}px";
            BorderThicknessValueText.Text = $"{_settings.BorderThickness}px";

            WindowOffsetXValueText.Text = $"{_settings.WindowOffsetX}px";
            WindowOffsetYValueText.Text = $"{_settings.WindowOffsetY}px";

            CaptureOffsetXValueText.Text = $"{_settings.CaptureOffsetX}px";
            CaptureOffsetYValueText.Text = $"{_settings.CaptureOffsetY}px";
        }

        private void RefreshProfileComboBox()
        {
            _isUpdatingControls = true;

            ProfileComboBox.Items.Clear();

            foreach(var profile in _appSettings.Profiles)
            {
                ProfileComboBox.Items.Add(profile.DisplayName);
            }

            ProfileComboBox.SelectedIndex = _appSettings.ActiveProfileIndex;

            _isUpdatingControls = false;
        }

        public event Action<Settings>? ActiveProfileChanged;
    }
}
