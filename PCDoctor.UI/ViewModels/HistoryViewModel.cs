using System.Collections.ObjectModel;
using System.Windows.Media;
using SystemStatsHistoryDto = PCDoctor.Core.Models.SystemStatsHistoryDto;

namespace PCDoctor.UI.ViewModels;

public class HistoryViewModel : BaseViewModel
{
    private const int FullHistoryCount = 25;

    public ObservableCollection<HistoryListItem> HistoryRows { get; } = new();

    private string historyCountText = "0 entries";
    public string HistoryCountText
    {
        get => historyCountText;
        set => SetProperty(ref historyCountText, value);
    }

    private string latestHistoryTimeText = "-";
    public string LatestHistoryTimeText
    {
        get => latestHistoryTimeText;
        set => SetProperty(ref latestHistoryTimeText, value);
    }

    private string latestHistoryUsageText = "-";
    public string LatestHistoryUsageText
    {
        get => latestHistoryUsageText;
        set => SetProperty(ref latestHistoryUsageText, value);
    }

    private string historyAlertText = "No data";
    public string HistoryAlertText
    {
        get => historyAlertText;
        set => SetProperty(ref historyAlertText, value);
    }

    private Brush historyAlertBrush = Brushes.Gray;
    public Brush HistoryAlertBrush
    {
        get => historyAlertBrush;
        set => SetProperty(ref historyAlertBrush, value);
    }

    public HistoryViewModel()
    {
        SetEmptyHistoryState();
    }

    public void Update(List<SystemStatsHistoryDto> history)
    {
        HistoryRows.Clear();

        List<SystemStatsHistoryDto> sortedHistory = history
            .OrderByDescending(item => item.CreatedAt)
            .Take(FullHistoryCount)
            .ToList();

        if (sortedHistory.Count == 0)
        {
            SetEmptyHistoryState();
            return;
        }

        for (int index = 0; index < sortedHistory.Count; index++)
        {
            HistoryRows.Add(HistoryListItem.Create(index + 1, sortedHistory[index]));
        }

        SystemStatsHistoryDto latestItem = sortedHistory[0];
        double latestMemoryPercent = CalculateMemoryUsagePercent(latestItem);

        HistoryCountText = $"{sortedHistory.Count} entries";
        LatestHistoryTimeText = latestItem.CreatedAt.ToString("HH:mm:ss");
        LatestHistoryUsageText = $"{latestItem.CpuUsage:F1}% / {latestMemoryPercent:F1}%";

        UpdateHistoryAlert(latestItem.CpuUsage, latestMemoryPercent);
    }

    private void SetEmptyHistoryState()
    {
        HistoryRows.Clear();

        HistoryCountText = "0 entries";
        LatestHistoryTimeText = "-";
        LatestHistoryUsageText = "-";
        HistoryAlertText = "No data";
        HistoryAlertBrush = Brushes.Gray;
    }

    private static double CalculateMemoryUsagePercent(SystemStatsHistoryDto item)
    {
        return item.TotalMemoryMb > 0
            ? item.UsedMemoryMb / item.TotalMemoryMb * 100
            : 0;
    }

    private void UpdateHistoryAlert(double cpuUsage, double memoryUsage)
    {
        double highestUsage = Math.Max(cpuUsage, memoryUsage);

        if (highestUsage >= 85)
        {
            HistoryAlertText = "Critical";
            HistoryAlertBrush = Brushes.IndianRed;
            return;
        }

        if (highestUsage >= 70)
        {
            HistoryAlertText = "Warning";
            HistoryAlertBrush = Brushes.Gold;
            return;
        }

        HistoryAlertText = "Normal";
        HistoryAlertBrush = Brushes.LightGreen;
    }
}