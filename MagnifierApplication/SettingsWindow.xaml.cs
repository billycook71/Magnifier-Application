using MagnifierApplication.Core;
using MagnifierApplication.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace MagnifierApplication
{
    ///Manages the settings interface, synchronizes controls with the active
    ///profile, applies capture presets, and persists configuration changes.
    public partial class SettingsWindow : Window
    {
        //prevents programmatic control updates from being treated as user input
        private bool _isUpdatingControls;

        //Work directly with the active profile's shared Settings instance.
        private Settings _settings;
        private readonly AppSettings _appSettings;
        private readonly SettingsStorageService _settingsStorage;

        private readonly StartupService _startupService = new();

        private readonly DispatcherTimer _saveDebounceTimer;

        private const int CustomCapturePresetIndex = 5;

        public SettingsWindow(AppSettings appSettings, SettingsStorageService settingsStorage)
        {
            InitializeComponent();

            _appSettings = appSettings;
            _settings = _appSettings.Profiles[_appSettings.ActiveProfileIndex].Settings;
            _settingsStorage = settingsStorage;

            //Restarted after each change so rapid slider movement produces one disk write.
            _saveDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _saveDebounceTimer.Tick += (s, e) =>
            {
                _saveDebounceTimer.Stop();
                _settingsStorage.Save(_appSettings);
            };

            RefreshProfileComboBox();
            LoadSettingsIntoControls();
            UpdateValueLabels();
            WireEvents();
        }

        ///Populates the settings controls from the active profile and global options
        ///without treating those assignments as user changes.
        private void LoadSettingsIntoControls()
        {
            _isUpdatingControls = true;

            StartHiddenCheckBox.IsChecked = _appSettings.StartHidden;
            StartWithWindowsCheckBox.IsChecked = _appSettings.StartWithWindows;

            MagnificationSlider.Value = _settings.Magnification;
            LensSizeSlider.Value = _settings.LensSize;
            WindowOffsetXSlider.Value = _settings.WindowOffsetX;
            WindowOffsetYSlider.Value = _settings.WindowOffsetY;
            BorderThicknessSlider.Value = _settings.BorderThickness;
            CaptureOffsetXSlider.Value = _settings.CaptureOffsetX;
            CaptureOffsetYSlider.Value = _settings.CaptureOffsetY;
            CapturePresetComboBox.SelectedIndex = _settings.CapturePresetIndex;

            ProfileComboBox.SelectedIndex =
                _appSettings.ActiveProfileIndex;

            ShapeComboBox.SelectedIndex =
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

                QueueSettingsSave();
            };


            MagnificationSlider.ValueChanged += (s, e) =>
            {
                _settings.Magnification = MagnificationSlider.Value;
                
                //Preset offsets scale with the capture region, which changes with zoom.
                if(_settings.CapturePresetIndex != CustomCapturePresetIndex)
                {
                    ApplySelectedCapturePreset();
                    UpdateCaptureOffsetControls();
                }

                UpdateValueLabels();
                QueueSettingsSave();
            };


            LensSizeSlider.ValueChanged += (s, e) =>
            {
                _settings.LensSize = (int)LensSizeSlider.Value;

                //Preset offsets scale with the capture region, which changes with zoom.
                if(_settings.CapturePresetIndex != CustomCapturePresetIndex)
                {
                    ApplySelectedCapturePreset();
                    UpdateCaptureOffsetControls();
                }
                
                UpdateValueLabels();
                QueueSettingsSave();
            };


            WindowOffsetXSlider.ValueChanged += (s, e) =>
            {
                _settings.WindowOffsetX = (int)WindowOffsetXSlider.Value;
                UpdateValueLabels();
                QueueSettingsSave();
            };


            WindowOffsetYSlider.ValueChanged += (s, e) =>
            {
                _settings.WindowOffsetY = (int)WindowOffsetYSlider.Value;
                UpdateValueLabels();
                QueueSettingsSave();
            };
                


            ShapeComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                if (ShapeComboBox.SelectedIndex == 0)
                    _settings.Shape = LensShape.Circle;
                    
                else
                    _settings.Shape = LensShape.Square;

                QueueSettingsSave();
            };

            SharpnessComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                if (SharpnessComboBox.SelectedIndex == 0)
                    _settings.RenderingMode = RenderingMode.Sharp;

                else
                    _settings.RenderingMode = RenderingMode.Smooth;

                QueueSettingsSave();
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

                QueueSettingsSave();
            };

            CapturePresetComboBox.SelectionChanged += (s, e) =>
            {
                if (_isUpdatingControls)
                    return;

                int selectedIndex = CapturePresetComboBox.SelectedIndex;

                if (selectedIndex < 0)
                    return;

                _settings.CapturePresetIndex = selectedIndex;

                if(selectedIndex == CustomCapturePresetIndex)
                {
                    QueueSettingsSave();
                    return;
                }

                ApplySelectedCapturePreset();
                UpdateCaptureOffsetControls();
                UpdateValueLabels();

                QueueSettingsSave();
            };

            CaptureOffsetXSlider.ValueChanged += (s, e) =>
            {
                _settings.CaptureOffsetX = (int)CaptureOffsetXSlider.Value;

                if (!_isUpdatingControls)
                {
                    _settings.CapturePresetIndex = CustomCapturePresetIndex;

                    _isUpdatingControls = true;
                    CapturePresetComboBox.SelectedIndex = CustomCapturePresetIndex;
                    _isUpdatingControls = false;
                } 

                UpdateValueLabels();
                QueueSettingsSave();
            };

            CaptureOffsetYSlider.ValueChanged += (s, e) =>
            {
                _settings.CaptureOffsetY = (int)CaptureOffsetYSlider.Value;

                if (!_isUpdatingControls)
                {
                    _settings.CapturePresetIndex = CustomCapturePresetIndex;

                    _isUpdatingControls= true;
                    CapturePresetComboBox.SelectedIndex = CustomCapturePresetIndex;
                    _isUpdatingControls = false;
                }
                    

                UpdateValueLabels();
                QueueSettingsSave();
            };

            BorderThicknessSlider.ValueChanged += (s, e) =>
            {
                _settings.BorderThickness = (int)BorderThicknessSlider.Value;
                UpdateValueLabels();
                QueueSettingsSave();

            };

        }

        //Refreshes the displayed values beside each numeric control.
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

        //Rebuilds the profile list and restores the active selection.
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

        private void StartHiddenCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingControls) 
                return;

            _appSettings.StartHidden = StartHiddenCheckBox.IsChecked == true;
            _settingsStorage.Save(_appSettings);
        }

        private void StartWithWindowsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingControls) 
                return;

            _appSettings.StartWithWindows = StartWithWindowsCheckBox.IsChecked == true;
            _startupService.SetStartWithWindows(_appSettings.StartWithWindows);
            _settingsStorage.Save(_appSettings);
        }

        //Calculates capture offsets for the selected preset using the current
        //lens size and magnification.
        private void ApplySelectedCapturePreset()
        {
            //The source region shrinks as magnification increases, so preset
            //offsets must be based on the current capture dimensions.
            int captureSize = Math.Max(
                1,
                (int)Math.Round(_settings.LensSize / _settings.Magnification)
                );

            int halfCaptureSize = captureSize / 2;

            switch (_settings.CapturePresetIndex)
            {
                case 0: //Centered on Cursor
                    _settings.CaptureOffsetX = -halfCaptureSize;
                    _settings.CaptureOffsetY = -halfCaptureSize;
                    break;

                case 1: //Above Cursor
                    _settings.CaptureOffsetX = -halfCaptureSize;
                    _settings.CaptureOffsetY = -captureSize;
                    break;

                case 2: //Below Cursor
                    _settings.CaptureOffsetX = -halfCaptureSize;
                    _settings.CaptureOffsetY = 0;
                    break;

                case 3: //Left of Cursor
                    _settings.CaptureOffsetX = -captureSize;
                    _settings.CaptureOffsetY = -halfCaptureSize;
                    break;

                case 4: //Right of Cursor
                    _settings.CaptureOffsetX = 0;
                    _settings.CaptureOffsetY = -halfCaptureSize;
                    break;

                case CustomCapturePresetIndex:
                    return;
            }
        }

        //Synchronizes the capture-offset sliders after a preset recalculates them.
        private void UpdateCaptureOffsetControls()
        {
            _isUpdatingControls = true;

            CaptureOffsetXSlider.Value = _settings.CaptureOffsetX;
            CaptureOffsetYSlider.Value = _settings.CaptureOffsetY;

            _isUpdatingControls = false;
        }

        //Restarts the save timere so rapid control changes are persisted as a
        //single settings write after input settles.
        private void QueueSettingsSave()
        {
            if (_isUpdatingControls)
                return;

            _saveDebounceTimer.Stop();
            _saveDebounceTimer.Start();
        }

        //Raised when the active profile changes so the magnifier engine
        //can begin using the newly selected Settings instance.
        public event Action<Settings>? ActiveProfileChanged;
    }
}
