using MagnifierApplication.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagnifierApplication.Services
{
    public class SettingsStorageService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public SettingsStorageService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            _settingsDirectory = Path.Combine(appDataPath, "MagnifierApplication");
            _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");
        }

        public AppSettings Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                AppSettings defaults = AppSettings.CreateDefault();
                Save(defaults);
                return defaults;
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);

                AppSettings? appSettings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

                return appSettings ?? AppSettings.CreateDefault();
            }
            catch
            {
                AppSettings defaults = AppSettings.CreateDefault();
                Save(defaults);
                return AppSettings.CreateDefault();
            }
        }

        public void Save(AppSettings appSettings)
        {
            Directory.CreateDirectory(_settingsDirectory);

            string json = JsonSerializer.Serialize(appSettings, _jsonOptions);

            File.WriteAllText(_settingsFilePath, json);
        }

    }
}
