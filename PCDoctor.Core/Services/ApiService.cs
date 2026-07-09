using System.Net.Http.Json;
using PCDoctor.Core.Models;
using PCDoctor.Models;
using Serilog;

namespace PCDoctor.Core.Services;

public class ApiService
{
    private readonly DeviceTokenStore deviceTokenStore = new();

    private HttpClient httpClient;
    private DeviceRegistrationService deviceRegistrationService;

    private DeviceRegistrationResponse? currentDevice;

    public DeviceRegistrationResponse? CurrentDevice => currentDevice;

    public bool IsApiAvailable { get; private set; }
    public DateTime? LastSuccessfulSyncAt { get; private set; }

    public ApiService(AppSettings settings)
    {
        httpClient = CreateHttpClient(settings.ApiBaseUrl);
        deviceRegistrationService = new DeviceRegistrationService(httpClient, deviceTokenStore);
    }

    public void UpdateApiBaseUrl(string apiBaseUrl)
    {
        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out _))
        {
            throw new ArgumentException("API base URL must be a valid absolute URL.");
        }

        httpClient = CreateHttpClient(apiBaseUrl);
        deviceRegistrationService = new DeviceRegistrationService(httpClient, deviceTokenStore);

        currentDevice = null;
        IsApiAvailable = false;
        LastSuccessfulSyncAt = null;

        Log.Information("API base URL updated to {ApiBaseUrl}.", apiBaseUrl);
    }

    public async Task SendSystemStatsAsync(SystemStats stats)
    {
        var dto = new
        {
            cpuUsage = stats.CpuUsage,
            usedMemoryMb = stats.UsedMemoryMB,
            totalMemoryMb = stats.TotalMemoryMB
        };

        try
        {
            DeviceRegistrationResponse device = await GetCurrentDeviceAsync();

            using HttpRequestMessage request =
                new(HttpMethod.Post, "/api/system-stats");

            request.Headers.Add("X-Device-Token", device.DeviceToken);
            request.Content = JsonContent.Create(dto);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            MarkApiAvailable();
            LastSuccessfulSyncAt = DateTime.Now;

            Log.Information("System stats sent successfully for device {DeviceId}.", device.Id);
        }
        catch (Exception e)
        {
            MarkApiUnavailable();
            Log.Error(e, "Failed to send system stats to API.");
        }
    }

    public async Task<List<SystemStatsHistoryDto>> GetHistoryAsync()
    {
        try
        {
            DeviceRegistrationResponse device = await GetCurrentDeviceAsync();

            List<SystemStatsHistoryDto>? history =
                await httpClient.GetFromJsonAsync<List<SystemStatsHistoryDto>>(
                    $"/api/devices/{device.Id}/system-stats/history"
                );

            MarkApiAvailable();

            return history ?? new List<SystemStatsHistoryDto>();
        }
        catch (Exception e)
        {
            MarkApiUnavailable();
            Log.Error(e, "Failed to fetch system stats history.");

            return new List<SystemStatsHistoryDto>();
        }
    }

    public async Task<List<DiagnosticMessageDto>> GetDiagnosticsAsync()
    {
        try
        {
            DeviceRegistrationResponse device = await GetCurrentDeviceAsync();

            List<DiagnosticMessageDto>? diagnostics =
                await httpClient.GetFromJsonAsync<List<DiagnosticMessageDto>>(
                    $"/api/devices/{device.Id}/system-stats/diagnostics"
                );

            MarkApiAvailable();

            return diagnostics ?? new List<DiagnosticMessageDto>();
        }
        catch (Exception e)
        {
            MarkApiUnavailable();
            Log.Error(e, "Failed to fetch diagnostics.");

            return new List<DiagnosticMessageDto>();
        }
    }

    public async Task ResetDeviceRegistrationAsync()
    {
        await deviceRegistrationService.ResetDeviceRegistrationAsync();

        currentDevice = null;
        IsApiAvailable = false;
        LastSuccessfulSyncAt = null;

        Log.Information("API service device registration cache was cleared.");
    }

    private async Task<DeviceRegistrationResponse> GetCurrentDeviceAsync()
    {
        if (currentDevice != null)
        {
            return currentDevice;
        }

        currentDevice = await deviceRegistrationService.GetOrRegisterDeviceAsync();

        return currentDevice;
    }

    private static HttpClient CreateHttpClient(string apiBaseUrl)
    {
        return new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        };
    }

    private void MarkApiAvailable()
    {
        IsApiAvailable = true;
    }

    private void MarkApiUnavailable()
    {
        IsApiAvailable = false;
    }
}