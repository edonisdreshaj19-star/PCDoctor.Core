using System.Text.Json;
using PCDoctor.Core.Models;

namespace PCDoctor.Core.Services;

public class SettingsService
{
    private const string SettingsFileName = "appsettings.json";

    public AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsFileName))
        {
            AppSettings defaultSettings = new();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }
        
        string json = File.ReadAllText(SettingsFileName);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFileName, json);
    }
}