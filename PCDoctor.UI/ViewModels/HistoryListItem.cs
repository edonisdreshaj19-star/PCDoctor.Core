using System.Windows.Media;
using SystemStatsHistoryDto = PCDoctor.Core.Models.SystemStatsHistoryDto;

namespace PCDoctor.UI.ViewModels;

public class HistoryListItem
{
    public string PositionText { get; }
    public string CreatedAtText { get; }
    public string CpuUsageText { get; }
    public double CpuUsageBarValue { get; }
    public Brush CpuUsageBrush { get; }
    public string MemoryUsageText { get; }
    public double MemoryUsageBarValue { get; }
    public Brush MemoryUsageBrush { get; }
    public string LevelText { get; }
    public Brush LevelBrush { get; }

    private HistoryListItem(
        string positionText,
        string createdAtText,
        string cpuUsageText,
        double cpuUsageBarValue,
        Brush cpuUsageBrush,
        string memoryUsageText,
        double memoryUsageBarValue,
        Brush memoryUsageBrush,
        string levelText,
        Brush levelBrush)
    {
        PositionText = positionText;
        CreatedAtText = createdAtText;
        CpuUsageText = cpuUsageText;
        CpuUsageBarValue = cpuUsageBarValue;
        CpuUsageBrush = cpuUsageBrush;
        MemoryUsageText = memoryUsageText;
        MemoryUsageBarValue = memoryUsageBarValue;
        MemoryUsageBrush = memoryUsageBrush;
        LevelText = levelText;
        LevelBrush = levelBrush;
    }

    public static HistoryListItem Create(int position, SystemStatsHistoryDto item)
    {
        double memoryUsagePercent = item.TotalMemoryMb > 0
            ? item.UsedMemoryMb / item.TotalMemoryMb * 100
            : 0;

        double highestUsage = Math.Max(item.CpuUsage, memoryUsagePercent);

        string levelText;
        Brush levelBrush;

        if (highestUsage >= 85)
        {
            levelText = "CRITICAL";
            levelBrush = Brushes.IndianRed;
        }
        else if (highestUsage >= 70)
        {
            levelText = "WARNING";
            levelBrush = Brushes.Gold;
        }
        else
        {
            levelText = "NORMAL";
            levelBrush = Brushes.LightGreen;
        }

        return new HistoryListItem(
            positionText: $"#{position}",
            createdAtText: item.CreatedAt.ToString("HH:mm:ss"),
            cpuUsageText: $"{item.CpuUsage:F1}%",
            cpuUsageBarValue: Math.Clamp(item.CpuUsage, 0, 100),
            cpuUsageBrush: GetUsageBrush(item.CpuUsage),
            memoryUsageText: $"{memoryUsagePercent:F1}%",
            memoryUsageBarValue: Math.Clamp(memoryUsagePercent, 0, 100),
            memoryUsageBrush: GetUsageBrush(memoryUsagePercent),
            levelText: levelText,
            levelBrush: levelBrush);
    }

    private static Brush GetUsageBrush(double usagePercent)
    {
        if (usagePercent >= 85)
        {
            return Brushes.IndianRed;
        }

        if (usagePercent >= 70)
        {
            return Brushes.Gold;
        }

        return Brushes.LightGreen;
    }
}