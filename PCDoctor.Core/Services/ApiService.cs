using PCDoctor.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using PCDoctor.Core.Models;

namespace PCDoctor.Core.Services;

public class ApiService
{
    private readonly HttpClient httpClient;

    public ApiService()
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080")
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
            throw; 
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
}