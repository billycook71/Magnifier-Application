using System;
using System.Runtime.InteropServices;

namespace MagnifierApplication.Services
{
    ///Registers and handles a global hotkey through the Windows API that toggles the magnifier lense
    internal class HotKeyService
    {
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_M = 0x4D;

        
        //Handle to the WPF Window that receives the WM_HOTKEY message
        private readonly IntPtr _handle;

        //Raised when the registered hotkey is pressed.
        public event Action? onHotkeyPressed;

        public HotKeyService(IntPtr handle)
        {
            _handle = handle;
        }

        //Registers the hotkey
        public void Register()
        {
            RegisterHotKey(_handle, HOTKEY_ID, MOD_CONTROL, VK_M); // Ctrl + M
        }

        //Releases the global hotkey registration during app shutdown
        public void Unregister()
        {
            UnregisterHotKey(_handle, HOTKEY_ID);
        }

        //Called from the MainWindow's native message hook
        //If the incoming message is from our hotkey, trigger event
        public void ProcessMessage(int msg, IntPtr wParam)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                onHotkeyPressed?.Invoke();
            }
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


    }
}
