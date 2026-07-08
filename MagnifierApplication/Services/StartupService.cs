using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace MagnifierApplication.Services
{
    internal class StartupService
    {
        private const string RunKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private const string AppName = "MagnifierApplication";

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

        public bool IsStartWithWindowsEnabled()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);

            if (key == null)
                return false;

            return key.GetValue(AppName) != null;
        }

        private void EnableStartup()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

            if(key == null)
                return;

            string executablePath = Process.GetCurrentProcess().MainModule?.FileName
                ?? Environment.ProcessPath
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(executablePath))
                return;

            string command = $"\"{executablePath}\" --hidden";

            key.SetValue(AppName, command);
        }

        private void DisableStartup()
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);

            if(key== null)
                return;

            key.DeleteValue(AppName, throwOnMissingValue: false);
        }
    }
}
