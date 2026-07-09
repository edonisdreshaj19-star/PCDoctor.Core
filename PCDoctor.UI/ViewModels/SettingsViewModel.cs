using System.Windows.Input;
using System.Windows.Media;
using PCDoctor.Core.Models;
using PCDoctor.Core.Services;
using PCDoctor.Models;
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
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand ResetDeviceRegistrationCommand { get; }

    private string apiBaseUrl;
    public string ApiBaseUrl
    {
        get => apiBaseUrl;
        set => SetProperty(ref apiBaseUrl, value);
    }

    private string refreshIntervalSeconds;
    public string RefreshIntervalSeconds
    {
        get => refreshIntervalSeconds;
        set => SetProperty(ref refreshIntervalSeconds, value);
    }

    private string apiSendIntervalSeconds;
    public string ApiSendIntervalSeconds
    {
        get => apiSendIntervalSeconds;
        set => SetProperty(ref apiSendIntervalSeconds, value);
    }

    private string statusMessage = string.Empty;
    public string StatusMessage
    {
        get => statusMessage;
        set => SetProperty(ref statusMessage, value);
    }

    private Brush statusBrush = Brushes.LightGreen;
    public Brush StatusBrush
    {
        get => statusBrush;
        set => SetProperty(ref statusBrush, value);
    }

    private string apiStatusText = "Unknown";
    public string ApiStatusText
    {
        get => apiStatusText;
        set => SetProperty(ref apiStatusText, value);
    }

    private Brush apiStatusBrush = Brushes.Gray;
    public Brush ApiStatusBrush
    {
        get => apiStatusBrush;
        set => SetProperty(ref apiStatusBrush, value);
    }

    private string lastSuccessfulSyncText = "-";
    public string LastSuccessfulSyncText
    {
        get => lastSuccessfulSyncText;
        set => SetProperty(ref lastSuccessfulSyncText, value);
    }

    private string deviceNameText = "Not registered";
    public string DeviceNameText
    {
        get => deviceNameText;
        set => SetProperty(ref deviceNameText, value);
    }

    private string deviceIdText = "-";
    public string DeviceIdText
    {
        get => deviceIdText;
        set => SetProperty(ref deviceIdText, value);
    }

    private string operatingSystemText = "-";
    public string OperatingSystemText
    {
        get => operatingSystemText;
        set => SetProperty(ref operatingSystemText, value);
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
        ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
        ResetDeviceRegistrationCommand = new RelayCommand(ResetDeviceRegistration);

        UpdateRuntimeStatus();
    }

    public void UpdateRuntimeStatus()
    {
        if (apiService.IsApiAvailable)
        {
            ApiStatusText = "Connected";
            ApiStatusBrush = Brushes.LightGreen;
        }
        else
        {
            ApiStatusText = "Offline";
            ApiStatusBrush = Brushes.IndianRed;
        }

        LastSuccessfulSyncText = apiService.LastSuccessfulSyncAt.HasValue
            ? apiService.LastSuccessfulSyncAt.Value.ToString("HH:mm:ss")
            : "-";

        DeviceRegistrationResponse? currentDevice = apiService.CurrentDevice;

        if (currentDevice == null)
        {
            DeviceNameText = "Not registered";
            DeviceIdText = "-";
            OperatingSystemText = "-";
            return;
        }

        DeviceNameText = string.IsNullOrWhiteSpace(currentDevice.DeviceName)
            ? "Unknown device"
            : currentDevice.DeviceName;

        DeviceIdText = currentDevice.Id.ToString();

        OperatingSystemText = string.IsNullOrWhiteSpace(currentDevice.OperatingSystem)
            ? "Unknown OS"
            : currentDevice.OperatingSystem;
    }

    private void SaveSettings()
    {
        StatusMessage = string.Empty;

        string trimmedApiBaseUrl = ApiBaseUrl.Trim();

        if (string.IsNullOrWhiteSpace(trimmedApiBaseUrl))
        {
            SetError("API Base URL is required.");
            return;
        }

        if (!Uri.TryCreate(trimmedApiBaseUrl, UriKind.Absolute, out _))
        {
            SetError("API Base URL must be a valid URL.");
            return;
        }

        if (!int.TryParse(RefreshIntervalSeconds, out int refreshInterval) || refreshInterval <= 0)
        {
            SetError("Refresh interval must be a positive number.");
            return;
        }

        if (!int.TryParse(ApiSendIntervalSeconds, out int apiSendInterval) || apiSendInterval <= 0)
        {
            SetError("API upload interval must be a positive number.");
            return;
        }

        try
        {
            settings.ApiBaseUrl = trimmedApiBaseUrl;
            settings.RefreshIntervalSeconds = refreshInterval;
            settings.ApiSendIntervalSeconds = apiSendInterval;

            settingsService.SaveSettings(settings);
            apiService.UpdateApiBaseUrl(settings.ApiBaseUrl);

            UpdateRuntimeStatus();

            SetSuccess("Settings saved. The API connection will refresh on the next sync.");

            CloseRequested?.Invoke(true);
        }
        catch (Exception e)
        {
            SetError($"Settings could not be saved: {e.Message}");
        }
    }

    private void ResetToDefaults()
    {
        AppSettings defaultSettings = new();

        ApiBaseUrl = defaultSettings.ApiBaseUrl;
        RefreshIntervalSeconds = defaultSettings.RefreshIntervalSeconds.ToString();
        ApiSendIntervalSeconds = defaultSettings.ApiSendIntervalSeconds.ToString();

        SetInfo("Default values loaded. Click Save to apply them.");
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

        try
        {
            await apiService.ResetDeviceRegistrationAsync();

            UpdateRuntimeStatus();

            SetSuccess("Device registration was reset successfully.");

            NotificationRequested?.Invoke(
                "Device registration was reset successfully.",
                "PCDoctor");

            CloseRequested?.Invoke(true);
        }
        catch (Exception e)
        {
            SetError($"Device registration could not be reset: {e.Message}");
        }
    }

    private void SetError(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.IndianRed;
    }

    private void SetSuccess(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.LightGreen;
    }

    private void SetInfo(string message)
    {
        StatusMessage = message;
        StatusBrush = Brushes.LightSkyBlue;
    }
}