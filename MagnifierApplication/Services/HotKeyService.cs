using Accessibility;
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
        //Arbitrary ID used by Windows to identify this registered hotkey
        private int _hotkeyId = 9000;
        //Handle to the WPF Window that receives the WM_HOTKEY message
        private IntPtr _handle;

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
            RegisterHotKey(_handle, _hotkeyId, 0x0002, 0x4D); // Ctrl + M
        }

        //Called from the window message loop
        //If the incoming message is from our hotkey, trigger event
        public void ProcessMessage(int msg, IntPtr wParam)
        {
            if (msg == 0x0312 && wParam.ToInt32() == _hotkeyId)
            {
                onHotkeyPressed?.Invoke();
            }
        }

        //Native Windows API function that registers a global hotkey
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd, int id, int fsModifiers, int vk);


    }
}
