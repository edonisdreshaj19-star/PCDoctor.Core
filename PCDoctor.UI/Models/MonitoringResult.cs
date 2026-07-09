using PCDoctor.Core.Models;
using PCDoctor.Models;

namespace PCDoctor.UI.Models;

public class MonitoringResult
{
    public SystemStats Stats { get; set; } = new();

    public List<SystemStatsHistoryDto> History { get; set; } = new();

    public List<DiagnosticMessageDto> Diagnostics { get; set; } = new();
    
    public bool IsApiAvailable { get; set; }

    public DateTime? LastSuccessfulSyncAt { get; set; }
}