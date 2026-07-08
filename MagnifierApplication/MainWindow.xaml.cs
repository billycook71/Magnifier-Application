using MagnifierApplication.Core;
using MagnifierApplication.Services;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Linq;

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using System.ComponentModel;

namespace MagnifierApplication
{
    ///Main overlay window for the magnifier
    ///This class is responsible for
    ///1.Creating shared services/settings
    ///2.Starting the render/update loop
    ///3.Positioning the lens near the cursor
    ///4.Responding to the global hotkey
    public partial class MainWindow : Window
    {
        private MagnifierEngine _engine;
        private CursorService _cursor;
        private HotKeyService? _hotkey;
        private Settings _settings;
        private SettingsStorageService _settingsStorage;
        private AppSettings _appSettings;

        //Determins whether lens is active
        private bool _enabled;

        //tray behavior
        private Forms.NotifyIcon? _trayIcon;
        private Forms.ToolStripMenuItem? _toggleMenuItem;
        private SettingsWindow? _settingsWindow;
        private bool _isExitRequested;


        public MainWindow()
        {
            InitializeComponent();

            //Shared between window and engine
            _cursor = new CursorService();

            //create one shared settings instance so UI and engine can read from the same configuration
            _settingsStorage = new SettingsStorageService();
            _appSettings = _settingsStorage.Load();
            _settings = _appSettings.Profiles[_appSettings.ActiveProfileIndex].Settings;

            //conform to app startup settings
            bool launchedHidden =
                Environment.GetCommandLineArgs()
                    .Any(arg => arg.Equals("--hidden", StringComparison.OrdinalIgnoreCase));

            _enabled = !(_appSettings.StartHidden || launchedHidden);
            Visibility = _enabled ? Visibility.Visible : Visibility.Hidden;

            //set window size from settings
            Width = _settings.LensSize;
            Height = _settings.LensSize;

            //build the magnifier pipeline
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

        ///Called after the WPF window has a real native window handle
        ///This is the time to register hooks and hotkeys tied to the handle
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);

            //Attach a custom messsage handler so we can recieve WM_HOTKEY messages
            source.AddHook(WndProc);

            //Register the global hotkey for toggling the magnifier
            _hotkey = new HotKeyService(helper.Handle);
            _hotkey.onHotkeyPressed += Toggle;
            _hotkey.Register();
        }

        ///Starts the timer-based update loop
        ///Each tick creates a new frame, places it in the lens, moves the window near the curosr
        private void StartLoop()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += (s, e) =>
            {
                if (!_enabled) return;

                UpdateLensVisuals();

                MagnifierImage.Source = _engine.UpdateFrame();
                UpdatePosition();
            };

            timer.Start();
        }

        ///Moves the magnifier window near the cursor
        ///Window offset from settings determines where the lens appears on the screen
        private void UpdatePosition()
        {
            var pos = _cursor.GetPosition();

            this.Left = pos.X + _settings.WindowOffsetX;
            this.Top = pos.Y + _settings.WindowOffsetY;
        }

        //Toggles the magnifier on and off
        private void Toggle()
        {
            _enabled = !_enabled;
            ApplyMagnifierVisibility();
        }

        private void UpdateToggleMenuText()
        {
            if (_toggleMenuItem != null)
            {
                _toggleMenuItem.Text = _enabled ? "Hide Magnifier" : "Show Magnifier";
            }
        }

        //Receives native windows messages for this window
        //Forwards relevant messages to the hotkey service
        private IntPtr WndProc(
            IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _hotkey?.ProcessMessage(msg, wParam);
            return IntPtr.Zero;
        }

        //Updates lens size/shape in real-time
        private void UpdateLensVisuals()
        {
            int shadowPadding = 24;

            Width = _settings.LensSize + shadowPadding;
            Height = _settings.LensSize+ shadowPadding;

            MagnifierImage.Width = _settings.LensSize;
            MagnifierImage.Height = _settings.LensSize;

            CircleBorder.Width = _settings.LensSize;
            CircleBorder.Height = _settings.LensSize;
            CircleBorder.StrokeThickness = _settings.BorderThickness;

            SquareBorder.Width = _settings.LensSize;
            SquareBorder.Height = _settings.LensSize;
            SquareBorder.BorderThickness = new Thickness(_settings.BorderThickness);


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

        private void ShowSettingsWindow()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow(_appSettings, _settingsStorage);

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

        private void ExitApplication()
        {
            _isExitRequested = true;

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if( !_isExitRequested)
            {
                e.Cancel = true;

                _enabled = false;
                Visibility = Visibility.Hidden;
                UpdateToggleMenuText();
                
                return;
            }

            base.OnClosing(e);
        }

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

        private void EnsureWindowHandleCreated()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
        }

        private void ApplyMagnifierVisibility()
        {
            Visibility = _enabled ? Visibility.Visible : Visibility.Hidden;
            UpdateToggleMenuText();
        }

        private Drawing.Icon LoadTrayIcon()
        {
            string iconPath = System.IO.Path.Combine(
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