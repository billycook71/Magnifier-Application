using MagnifierApplication.Core;
using System;
using System.Collections.Generic;
using System.Text;
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

        public Settings Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new Settings();
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);

                Settings? settings = JsonSerializer.Deserialize<Settings>(json, _jsonOptions);

                return settings ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public void Save(Settings settings)
        {
            Directory.CreateDirectory(_settingsDirectory);

            string json = JsonSerializer.Serialize(settings, _jsonOptions);

            File.WriteAllText(_settingsFilePath, json);
        }

    }
}
