using MagnifierApplication.Core;
using MagnifierApplication.Services;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Linq;

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace MagnifierApplication
{
    ///Coordinates the magnifier overlay, rendering loop, global hotkey,
    ///settings window, tray behavior, and applicaion lifecycle.
    public partial class MainWindow : Window
    {
        private MagnifierEngine _engine;
        private CursorService _cursor;
        private HotKeyService? _hotkey;
        private Settings _settings;
        private SettingsStorageService _settingsStorage;
        private AppSettings _appSettings;

        //Current runtime state of the magnifier lens.
        private bool _enabled;

        //Tray UI and applicaion-lifecycle state.
        private Forms.NotifyIcon? _trayIcon;
        private Forms.ToolStripMenuItem? _toggleMenuItem;
        private SettingsWindow? _settingsWindow;
        private bool _isExitRequested;


        public MainWindow()
        {
            InitializeComponent();

            //Shared cursor service used by both positioning and frame capture.
            _cursor = new CursorService();

            //Load one shared settings graph so the engine and settings window
            //operate on the same in-memory configuration.
            _settingsStorage = new SettingsStorageService();
            _appSettings = _settingsStorage.Load();
            _settings = _appSettings.Profiles[_appSettings.ActiveProfileIndex].Settings;

            //Windows startup uses --hidden so the utility launches quietly in the tray.
            bool launchedHidden =
                Environment.GetCommandLineArgs()
                    .Any(arg => arg.Equals("--hidden", StringComparison.OrdinalIgnoreCase));

            //Respect the user's normal startup preference and force hidden launches.
            _enabled = !(_appSettings.StartHidden || launchedHidden);
            Visibility = _enabled ? Visibility.Visible : Visibility.Hidden;

            //Initialize the lens using the active profile.
            Width = _settings.LensSize;
            Height = _settings.LensSize;

            //Build the screen capture and rendering pipeline.
            _engine = new MagnifierEngine(
                _cursor,
                new ScreenCaptureService(),
                new Rendering.MagnifierRenderer(),
                _settings
                );


            InitializeTrayIcon();
            UpdateToggleMenuText();

            EnsureWindowHandleCreated();
            ApplyMagnifierVisibility();


            StartLoop();
        }

        ///Attaches the native Windows message hook and registers the global
        ///hotkey after WPF creates the window handle.
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);

            //Attach a native message hook so WM_HOTKEY message reach the service.
            source.AddHook(WndProc);

            //Register Ctrl+M against the window handle.
            _hotkey = new HotKeyService(helper.Handle);
            _hotkey.onHotkeyPressed += Toggle;
            _hotkey.Register();
        }

        ///Starts the timer-driven rendering loop. When the lens is enabled,
        ///each tick captures a frame, updates the overlay, and follows the cursor.
        private void StartLoop()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += (s, e) =>
            {
                //Skip capture and rendering entirely while the lens is hidden.
                if (!_enabled) return;

                UpdateLensVisuals();

                MagnifierImage.Source = _engine.UpdateFrame();
                UpdatePosition();
            };

            timer.Start();
        }

        ///Positions the lens relative to the cursor using the active profile offsets.
        private void UpdatePosition()
        {
            var pos = _cursor.GetPosition();

            Left = pos.X + _settings.WindowOffsetX;
            Top = pos.Y + _settings.WindowOffsetY;
        }

        //Toggles the current lens state through the shared visibility path.
        private void Toggle()
        {
            _enabled = !_enabled;
            ApplyMagnifierVisibility();
        }

        //Keeps the tray action label synchronized with the lens state.
        private void UpdateToggleMenuText()
        {
            if (_toggleMenuItem != null)
            {
                _toggleMenuItem.Text = _enabled ? "Hide Magnifier" : "Show Magnifier";
            }
        }

        //Forwards native window messages to the hotkey service.
        private IntPtr WndProc(
            IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hotkey?.ProcessMessage(msg, wParam);
            return IntPtr.Zero;
        }

        //Applies the active profile's lens size, border thickness, and shape
        //to the transparent overlay.
        private void UpdateLensVisuals()
        {
            //Extra transparent space prevents the drop shadow from being clipped.
            const int shadowPadding = 24;

            //Resize the overlay and visible lens elements.
            Width = _settings.LensSize + shadowPadding;
            Height = _settings.LensSize + shadowPadding;

            MagnifierImage.Width = _settings.LensSize;
            MagnifierImage.Height = _settings.LensSize;

            CircleBorder.Width = _settings.LensSize;
            CircleBorder.Height = _settings.LensSize;
            CircleBorder.StrokeThickness = _settings.BorderThickness;

            SquareBorder.Width = _settings.LensSize;
            SquareBorder.Height = _settings.LensSize;
            SquareBorder.BorderThickness = new Thickness(_settings.BorderThickness);

            //Apply shape-specific clipping and border visibility.
            if (_settings.Shape == LensShape.Circle)
            {
                MagnifierImage.Clip = new EllipseGeometry(
                    new System.Windows.Point(_settings.LensSize / 2.0, _settings.LensSize / 2.0),
                    _settings.LensSize / 2.0,
                    _settings.LensSize / 2.0);

                CircleBorder.Visibility = Visibility.Visible;
                SquareBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                MagnifierImage.Clip = null;
                CircleBorder.Visibility = Visibility.Collapsed;
                SquareBorder.Visibility = Visibility.Visible;
            }
        }

        //Creates the system-tray icon and its Settings, Show/Hide, and Exit actions.
        private void InitializeTrayIcon()
        {
            _toggleMenuItem = new Forms.ToolStripMenuItem("Hide Magnifier");
            _toggleMenuItem.Click += (s, e) => Toggle();

            var settingsMenuItem = new Forms.ToolStripMenuItem("Settings");
            settingsMenuItem.Click += (s, e) => ShowSettingsWindow();

            var exitMenuItem = new Forms.ToolStripMenuItem("Exit");
            exitMenuItem.Click += (s, e) => ExitApplication();

            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add(settingsMenuItem);
            contextMenu.Items.Add(_toggleMenuItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            _trayIcon = new Forms.NotifyIcon
            {
                Text = "Magnifier Application",
                Icon = LoadTrayIcon(),
                ContextMenuStrip = contextMenu,
                Visible = true
            };

            _trayIcon.DoubleClick += (s, e) => ShowSettingsWindow();
        }

        //Opens the settings window or focuses the existing instance if already open.
        private void ShowSettingsWindow()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(_appSettings, _settingsStorage);

            //Keep the overlay and rendering engine synchronized with the profile changes.
            _settingsWindow.ActiveProfileChanged += settings =>
            {
                _settings = settings;
                _engine.Settings = _settings;
            };

            _settingsWindow.Closed += (s, e) =>
            {
                _settingsWindow = null;
            };

            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        //Performs the explicit tray-menu shutdown path.
        private void ExitApplication()
        {
            //Allow OnClosing to distinguish an intentional exit from hide-to-tray behavior.
            _isExitRequested = true;

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            Close();
        }
        
        //Converts normal window closure into hide-to-tray behavior unless
        //the user explicitly selected Exit.
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExitRequested)
            {
                e.Cancel = true;

                _enabled = false;
                Visibility = Visibility.Hidden;
                UpdateToggleMenuText();
                
                return;
            }

            base.OnClosing(e);
        }

        //Release the operating-system resources after the application fully closes.
        protected override void OnClosed(EventArgs e)
        {
            _hotkey?.Unregister();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            base.OnClosed(e);
        }

        //Forces creation of the native window handle so the global hotkey can
        //register even when the overlay starts hidden.
        private void EnsureWindowHandleCreated()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
        }

        //Applies the current lens state to both the overlay and tray menu.
        private void ApplyMagnifierVisibility()
        {
            Visibility = _enabled ? Visibility.Visible : Visibility.Hidden;
            UpdateToggleMenuText();
        }

        //Loads the packaged tray icon, falling back to the Windows applicaiton
        //icon if the asset cannot be found.
        private Drawing.Icon LoadTrayIcon()
        {
            string iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "MagnifierApplication.ico"
            );

            if (File.Exists(iconPath))
            {
                return new Drawing.Icon(iconPath);
            }

            return Drawing.SystemIcons.Application;
        }
    }
}