using PCDoctor.Models;

using System.Net.Http.Json;
using PCDoctor.Core.Models;

namespace PCDoctor.Core.Services;

public class ApiService
{
    private readonly HttpClient httpClient;
    private readonly AppSettings settings;

    public ApiService(AppSettings settings)
    {
        this.settings = settings;
        httpClient = new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl)
        };
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
            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync("/api/system-stats", dto);
                
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public async Task<List<SystemStatsHistoryDto>> GetHistoryAsync()
    {
        try
        {
            List<SystemStatsHistoryDto>? history =
                await httpClient.GetFromJsonAsync<List<SystemStatsHistoryDto>>(
                    "/api/system-stats/history"
                );

            return history ?? new List<SystemStatsHistoryDto>();
        }
        catch
        {
            return new List<SystemStatsHistoryDto>();
        }
    }
    
    public async Task<List<DiagnosticMessageDto>> GetDiagnosticsAsync()
    {
        try
        {
            List<DiagnosticMessageDto>? diagnostics =
                await httpClient.GetFromJsonAsync<List<DiagnosticMessageDto>>(
                    "/api/system-stats/diagnostics"
                );

            return diagnostics ?? new List<DiagnosticMessageDto>();
        }
        catch
        {
            return new List<DiagnosticMessageDto>();
        }
    }
}