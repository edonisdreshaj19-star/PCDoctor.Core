using System.Windows.Media;

namespace PCDoctor.UI.ViewModels;

public class ProcessListItem
{
    public string PositionText { get; }
    public string ProcessName { get; }
    public int ProcessId { get; }
    public string MemoryUsageText { get; }
    public double MemoryUsageBarValue { get; }
    public string MemoryLevelText { get; }
    public Brush MemoryUsageBrush { get; }

    private ProcessListItem(
        string positionText,
        string processName,
        int processId,
        string memoryUsageText,
        double memoryUsageBarValue,
        string memoryLevelText,
        Brush memoryUsageBrush)
    {
        PositionText = positionText;
        ProcessName = processName;
        ProcessId = processId;
        MemoryUsageText = memoryUsageText;
        MemoryUsageBarValue = memoryUsageBarValue;
        MemoryLevelText = memoryLevelText;
        MemoryUsageBrush = memoryUsageBrush;
    }

    public static ProcessListItem Create(
        int position,
        string processName,
        int processId,
        double memoryUsageMb,
        double highestMemoryUsageMb)
    {
        double barValue = highestMemoryUsageMb <= 0
            ? 0
            : Math.Min(100, memoryUsageMb / highestMemoryUsageMb * 100);

        string memoryLevelText;
        Brush memoryUsageBrush;

        if (memoryUsageMb >= 2000)
        {
            memoryLevelText = "HEAVY";
            memoryUsageBrush = Brushes.IndianRed;
        }
        else if (memoryUsageMb >= 1000)
        {
            memoryLevelText = "HIGH";
            memoryUsageBrush = Brushes.Gold;
        }
        else if (memoryUsageMb >= 500)
        {
            memoryLevelText = "MEDIUM";
            memoryUsageBrush = Brushes.LightSkyBlue;
        }
        else
        {
            memoryLevelText = "LOW";
            memoryUsageBrush = Brushes.LightGreen;
        }

        return new ProcessListItem(
            positionText: $"#{position}",
            processName: string.IsNullOrWhiteSpace(processName) ? "Unknown" : processName,
            processId: processId,
            memoryUsageText: $"{memoryUsageMb:F1} MB",
            memoryUsageBarValue: barValue,
            memoryLevelText: memoryLevelText,
            memoryUsageBrush: memoryUsageBrush);
    }
}