using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MagnifierApplication.Services
{
    ///Registers and handles a global hotkey through the Windows API
    ///Currently hardcoded to CTRL+M but will be changeable through the GUI
    internal class HotKeyService
    {
        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_M = 0x4D;

        
        //Handle to the WPF Window that receives the WM_HOTKEY message
        private readonly IntPtr _handle;

        //Raised when the registered hotkey is pressed.
        //MainWindow uses this to toggle the magnifier
        public event Action? onHotkeyPressed;

        public HotKeyService(IntPtr handle)
        {
            _handle = handle;
        }

        //Registers the hotkey, default (hardcoded)
        public void Register()
        {
            RegisterHotKey(_handle, HOTKEY_ID, MOD_CONTROL, VK_M); // Ctrl + M
        }

        public void Unregister()
        {
            UnregisterHotKey(_handle, HOTKEY_ID);
        }

        //Called from the window message loop
        //If the incoming message is from our hotkey, trigger event
        public void ProcessMessage(int msg, IntPtr wParam)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                onHotkeyPressed?.Invoke();
            }
        }

        //Native Windows API function that registers a global hotkey
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd, int id, int fsModifiers, int vk);

        //Native Windows API function to unregister a global hotkey
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


    }
}
