using PCDoctor.Core.Models;
using PCDoctor.Core.Services;

namespace PCDoctor.UI.Services;

public class WindowService
{
    private readonly AppSettings settings;
    private readonly SettingsService settingsService;

    public WindowService(AppSettings settings, SettingsService settingsService)
    {
        this.settings = settings;
        this.settingsService = settingsService;
    }

    public void OpenSettingsWindow()
    {
        SettingsWindow settingsWindow = new(settings, settingsService);
        settingsWindow.ShowDialog();
    }
}