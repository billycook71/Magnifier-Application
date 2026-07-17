using MagnifierApplication.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagnifierApplication.Services
{
    ///Loads and saves the application's persistent settings from the
    ///user's local AppData directory.
    public class SettingsStorageService
    {
        //Directory containing the application's persistent settings.
        private readonly string _settingsDirectory;

        //Full path to the settings JSON file.
        private readonly string _settingsFilePath;

        //Shared serializer configuration used for both the loading and saving.
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


        ///Loads the application's settings from disk.
        ///If no settings file exists or loading fails, a default configuration
        ///is created and saved.
        public AppSettings Load()
        {
            if (!File.Exists(_settingsFilePath))
            {
                //First launch: create and persist a default settings file.
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
                //Recover from a missing or corrupted settings file.
                AppSettings defaults = AppSettings.CreateDefault();
                Save(defaults);
                return defaults;
            }
        }

        //Save the current application settings to disk
        public void Save(AppSettings appSettings)
        {
            //Ensure the settings directory exists before writing the file.
            Directory.CreateDirectory(_settingsDirectory);

            string json = JsonSerializer.Serialize(appSettings, _jsonOptions);

            File.WriteAllText(_settingsFilePath, json);
        }

    }
}
