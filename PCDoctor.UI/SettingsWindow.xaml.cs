using System.Windows;
using PCDoctor.Core.Models;
using PCDoctor.Core.Services;

namespace PCDoctor.UI;

public partial class SettingsWindow : Window
{
    private readonly AppSettings settings;
    private readonly SettingsService settingsService;
    
    public SettingsWindow(AppSettings settings, SettingsService settingsService)
    {
        InitializeComponent();

        this.settings = settings;
        this.settingsService = settingsService;

        LoadSettingsToUi();
    }

    private void LoadSettingsToUi()
    {
        ApiBaseUrlTextBox.Text = settings.ApiBaseUrl;
        RefreshIntervalTextBox.Text = settings.RefreshIntervalSeconds.ToString();
        ApiBaseUrlTextBox.Text = settings.ApiSendIntervalSeconds.ToString();
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        settings.ApiBaseUrl = ApiBaseUrlTextBox.Text.Trim();
        
        if(int.TryParse(RefreshIntervalTextBox.Text, out int refreshInterval))
        {
            settings.RefreshIntervalSeconds = refreshInterval;
        }
        
        if(int.TryParse(ApiSendIntervalTextBox.Text, out int apiSendInterval))
        {
            settings.ApiSendIntervalSeconds = apiSendInterval;
        }

        settingsService.SaveSettings(settings);
        
        DialogResult = true;
        Close();
    }
}