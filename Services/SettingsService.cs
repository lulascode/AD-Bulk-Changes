using System;
using System.IO;
using System.Text.Json;
using AD_BulkChanges.Models;

namespace AD_BulkChanges.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        
        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "AD-BulkChanges");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
        }
        
        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden der Einstellungen: {ex.Message}");
            }
            
            return new AppSettings();
        }
        
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern der Einstellungen: {ex.Message}");
            }
        }
    }
}
