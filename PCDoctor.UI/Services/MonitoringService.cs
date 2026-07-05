using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.Services;

public class MonitoringService
{
    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;

    private DateTime lastApiSendTime = DateTime.MinValue;
    
    public MonitoringService(AppSettings settings, SystemMonitor monitor, ApiService apiService)
    {
        this.settings = settings;
        this.monitor = monitor;
        this.apiService = apiService;
    }
    
    public async Task<MonitoringResult> GetMonitoringResultAsync()
    {
        SystemStats stats = monitor.GetStats();

        List<SystemStatsHistoryDto> history = await apiService.GetHistoryAsync();
        List<DiagnosticMessageDto> diagnostics = await apiService.GetDiagnosticsAsync();

        await SendStatsIfNeededAsync(stats);

        return new MonitoringResult
        {
            Stats = stats,
            History = history,
            Diagnostics = diagnostics
        };
    }
    
    private async Task SendStatsIfNeededAsync(SystemStats stats)
    {
        if ((DateTime.Now - lastApiSendTime).TotalSeconds < settings.ApiSendIntervalSeconds)
        {
            return;
        }

        await apiService.SendSystemStatsAsync(stats);
        lastApiSendTime = DateTime.Now;
    }
}