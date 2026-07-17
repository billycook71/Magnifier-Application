using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace MagnifierApplication.Services
{
    ///Manages the application's Window startup registration by adding
    ///or removing an entry from the current user's Run registry key.
    internal class StartupService
    {
        //Registry location containing the applications that launch at user sign-in.
        private const string RunKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        //Registry value name used by this application.
        private const string AppName = "MagnifierApplication";

        //Enables or disables automatic startup with Windows.
        public void SetStartWithWindows(bool enabled)
        {
            if (enabled)
            {
                EnableStartup();
            }
            else
            {
                DisableStartup();
            }
        }

        ///Returns whether the application is currently registered to
        ///start automatically with Windows.
        public bool IsStartWithWindowsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);

            if (key == null)
                return false;

            return key.GetValue(AppName) != null;
        }

        ///Register's the application's executable in the current user's
        ///Windows Run registry key.
        private void EnableStartup()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

            if (key == null)
                return;

            //Resolve the currently running executable so packaged and
            //development builds reigster the correct path.
            string executablePath = Process.GetCurrentProcess().MainModule?.FileName
                ?? Environment.ProcessPath
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(executablePath))
                return;

            //Launch hidden so the tray utility starts without immediately
            //displaying the magnifier lens.
            string command = $"\"{executablePath}\" --hidden";

            key.SetValue(AppName, command);
        }

        //Removes the applicaion's startup registration from Windows.
        private void DisableStartup()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

            if (key == null)
                return;

            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }
}
