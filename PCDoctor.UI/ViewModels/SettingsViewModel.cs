using System.Windows.Input;
using PCDoctor.Core.Models;
using PCDoctor.Core.Services;
using PCDoctor.UI.Commands;

namespace PCDoctor.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly AppSettings settings;
    private readonly SettingsService settingsService;
    private readonly ApiService apiService;

    public event Action<bool?>? CloseRequested;
    public event Func<string, string, bool>? ConfirmationRequested;
    public event Action<string, string>? NotificationRequested;

    public ICommand SaveCommand { get; }
    public ICommand ResetDeviceRegistrationCommand { get; }

    private string apiBaseUrl;
    public string ApiBaseUrl
    {
        get => apiBaseUrl;
        set
        {
            apiBaseUrl = value;
            OnPropertyChanged();
        }
    }

    private string refreshIntervalSeconds;
    public string RefreshIntervalSeconds
    {
        get => refreshIntervalSeconds;
        set
        {
            refreshIntervalSeconds = value;
            OnPropertyChanged();
        }
    }

    private string apiSendIntervalSeconds;
    public string ApiSendIntervalSeconds
    {
        get => apiSendIntervalSeconds;
        set
        {
            apiSendIntervalSeconds = value;
            OnPropertyChanged();
        }
    }

    private string statusMessage = string.Empty;
    public string StatusMessage
    {
        get => statusMessage;
        set
        {
            statusMessage = value;
            OnPropertyChanged();
        }
    }

    public SettingsViewModel(
        AppSettings settings,
        SettingsService settingsService,
        ApiService apiService)
    {
        this.settings = settings;
        this.settingsService = settingsService;
        this.apiService = apiService;

        apiBaseUrl = settings.ApiBaseUrl;
        refreshIntervalSeconds = settings.RefreshIntervalSeconds.ToString();
        apiSendIntervalSeconds = settings.ApiSendIntervalSeconds.ToString();

        SaveCommand = new RelayCommand(SaveSettings);
        ResetDeviceRegistrationCommand = new RelayCommand(ResetDeviceRegistration);
    }

    private void SaveSettings()
    {
        StatusMessage = string.Empty;

        string trimmedApiBaseUrl = ApiBaseUrl.Trim();

        if (string.IsNullOrWhiteSpace(trimmedApiBaseUrl))
        {
            StatusMessage = "API Base URL is required.";
            return;
        }

        if (!Uri.TryCreate(trimmedApiBaseUrl, UriKind.Absolute, out _))
        {
            StatusMessage = "API Base URL must be a valid URL.";
            return;
        }

        if (!int.TryParse(RefreshIntervalSeconds, out int refreshInterval) || refreshInterval <= 0)
        {
            StatusMessage = "Refresh interval must be a positive number.";
            return;
        }

        if (!int.TryParse(ApiSendIntervalSeconds, out int apiSendInterval) || apiSendInterval <= 0)
        {
            StatusMessage = "API upload interval must be a positive number.";
            return;
        }

        settings.ApiBaseUrl = trimmedApiBaseUrl;
        settings.RefreshIntervalSeconds = refreshInterval;
        settings.ApiSendIntervalSeconds = apiSendInterval;

        settingsService.SaveSettings(settings);
        apiService.UpdateApiBaseUrl(settings.ApiBaseUrl);

        CloseRequested?.Invoke(true);
    }

    private async void ResetDeviceRegistration()
    {
        bool confirmed = ConfirmationRequested?.Invoke(
            "This will delete the local device registration. PCDoctor will register as a new device on the next sync. Continue?",
            "Reset Device Registration") ?? false;

        if (!confirmed)
        {
            return;
        }

        await apiService.ResetDeviceRegistrationAsync();

        NotificationRequested?.Invoke(
            "Device registration was reset successfully.",
            "PCDoctor");

        CloseRequested?.Invoke(true);
    }
}