using PCDoctor.Core.Models;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.Services;

public class DashboardFormatter
{
    public string FormatDisk(DiskStats disk)
    {
        return $"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)";
    }

    public string FormatProcess(ProcessStats process)
    {
        return $"{process.ProcessName} ({process.ProcessId}) - {process.MemoryUsageMB:F1} MB";
    }

    public string FormatHistory(SystemStatsHistoryDto item)
    {
        double memoryPercent =
            item.TotalMemoryMb > 0
                ? item.UsedMemoryMb / item.TotalMemoryMb * 100
                : 0;

        return $"{item.CreatedAt:HH:mm:ss} | CPU {item.CpuUsage:F1}% | RAM {memoryPercent:F1}%";
    }

    public string FormatDiagnostic(DiagnosticMessageDto diagnostic)
    {
        return $"{diagnostic.Level}: {diagnostic.Message}";
    }

    public string FormatMemory(SystemStats stats)
    {
        return $"{stats.UsedMemoryMB / 1024:F2} GB / {stats.TotalMemoryMB / 1024:F2} GB";
    }

    public double CalculateMemoryUsagePercent(SystemStats stats)
    {
        return stats.TotalMemoryMB > 0
            ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
            : 0;
    }
}