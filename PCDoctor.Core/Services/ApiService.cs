using PCDoctor.Models;

using System.Net.Http.Json;
using PCDoctor.Core.Models;
using Serilog;

namespace PCDoctor.Core.Services;

public class ApiService
{
    private readonly HttpClient httpClient;
    private readonly DeviceRegistrationService deviceRegistrationService;

    private DeviceRegistrationResponse? currentDevice;

    public ApiService(AppSettings settings)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl)
        };

        DeviceTokenStore deviceTokenStore = new();
        deviceRegistrationService = new DeviceRegistrationService(httpClient, deviceTokenStore);
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
            Log.Information("System stats sent successfully for device {DeviceId}.", device.Id);
        }
        catch (Exception e)
        {
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

            return history ?? new List<SystemStatsHistoryDto>();
        }
        catch (Exception e)
        {
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

            return diagnostics ?? new List<DiagnosticMessageDto>();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to fetch diagnostics.");
            return new List<DiagnosticMessageDto>();
        }
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
}