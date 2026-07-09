using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PCDoctor.Core.Models;
using PCDoctor.Models;
using PCDoctor.UI.Models;
using PCDoctor.UI.Services;
using SkiaSharp;

namespace PCDoctor.UI.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private const int MaxCpuChartPoints = 30;
    private const int ProcessPreviewCount = 3;
    private const int HistoryPreviewCount = 3;
    private const int FullHistoryCount = 25;
    private const int DiskPreviewCount = 3;

    private readonly DashboardFormatter formatter;
    private readonly ObservableCollection<double> cpuValues = new();

    public ObservableCollection<string> DiskItems { get; } = new();
    public ObservableCollection<string> DiskPreviewItems { get; } = new();
    public ObservableCollection<string> ProcessItems { get; } = new();
    public ObservableCollection<string> ProcessPreviewItems { get; } = new();
    public ObservableCollection<ProcessListItem> ProcessRows { get; } = new();
    public ObservableCollection<string> HistoryItems { get; } = new();
    public ObservableCollection<string> HistoryPreviewItems { get; } = new();
    public ObservableCollection<HistoryListItem> HistoryRows { get; } = new();
    public ObservableCollection<string> DiagnosticItems { get; } = new();
    public ObservableCollection<string> HealthReasonItems { get; } = new();
    public ObservableCollection<string> HealthRecommendationItems { get; } = new();

    public ObservableCollection<string> ReportDetectedIssueItems { get; } = new();
    public ObservableCollection<string> ReportRecommendationItems { get; } = new();

    private string cpuUsageText = "0%";
    public string CpuUsageText
    {
        get => cpuUsageText;
        set => SetProperty(ref cpuUsageText, value);
    }

    private double cpuProgressValue;
    public double CpuProgressValue
    {
        get => cpuProgressValue;
        set => SetProperty(ref cpuProgressValue, value);
    }

    private Brush cpuProgressBrush = Brushes.LightGreen;
    public Brush CpuProgressBrush
    {
        get => cpuProgressBrush;
        set => SetProperty(ref cpuProgressBrush, value);
    }

    private string memoryUsageText = "0 GB / 0 GB";
    public string MemoryUsageText
    {
        get => memoryUsageText;
        set => SetProperty(ref memoryUsageText, value);
    }

    private double memoryProgressValue;
    public double MemoryProgressValue
    {
        get => memoryProgressValue;
        set => SetProperty(ref memoryProgressValue, value);
    }

    private Brush memoryProgressBrush = Brushes.LightGreen;
    public Brush MemoryProgressBrush
    {
        get => memoryProgressBrush;
        set => SetProperty(ref memoryProgressBrush, value);
    }

    private string apiStatusText = "● API: Unknown";
    public string ApiStatusText
    {
        get => apiStatusText;
        set => SetProperty(ref apiStatusText, value);
    }

    private Brush apiStatusBrush = Brushes.Gray;
    public Brush ApiStatusBrush
    {
        get => apiStatusBrush;
        set => SetProperty(ref apiStatusBrush, value);
    }

    private string lastSyncText = "Last Sync: -";
    public string LastSyncText
    {
        get => lastSyncText;
        set => SetProperty(ref lastSyncText, value);
    }

    private string deviceNameText = "Device: -";
    public string DeviceNameText
    {
        get => deviceNameText;
        set => SetProperty(ref deviceNameText, value);
    }

    private string deviceIdText = "ID: -";
    public string DeviceIdText
    {
        get => deviceIdText;
        set => SetProperty(ref deviceIdText, value);
    }

    private string operatingSystemText = "OS: -";
    public string OperatingSystemText
    {
        get => operatingSystemText;
        set => SetProperty(ref operatingSystemText, value);
    }

    private string healthScoreText = "-- / 100";
    public string HealthScoreText
    {
        get => healthScoreText;
        set => SetProperty(ref healthScoreText, value);
    }

    private double healthProgressValue;
    public double HealthProgressValue
    {
        get => healthProgressValue;
        set => SetProperty(ref healthProgressValue, value);
    }

    private string healthStatusText = "UNKNOWN";
    public string HealthStatusText
    {
        get => healthStatusText;
        set => SetProperty(ref healthStatusText, value);
    }

    private string healthSummaryText = "No health data available.";
    public string HealthSummaryText
    {
        get => healthSummaryText;
        set => SetProperty(ref healthSummaryText, value);
    }

    private Brush healthStatusBrush = Brushes.Gray;
    public Brush HealthStatusBrush
    {
        get => healthStatusBrush;
        set => SetProperty(ref healthStatusBrush, value);
    }

    private string diagnosticReportScoreText = "-- / 100";
    public string DiagnosticReportScoreText
    {
        get => diagnosticReportScoreText;
        set => SetProperty(ref diagnosticReportScoreText, value);
    }

    private double diagnosticReportProgressValue;
    public double DiagnosticReportProgressValue
    {
        get => diagnosticReportProgressValue;
        set => SetProperty(ref diagnosticReportProgressValue, value);
    }

    private string diagnosticReportStatusText = "NO REPORT";
    public string DiagnosticReportStatusText
    {
        get => diagnosticReportStatusText;
        set => SetProperty(ref diagnosticReportStatusText, value);
    }

    private Brush diagnosticReportStatusBrush = Brushes.Gray;
    public Brush DiagnosticReportStatusBrush
    {
        get => diagnosticReportStatusBrush;
        set => SetProperty(ref diagnosticReportStatusBrush, value);
    }

    private string diagnosticReportSummaryText = "No diagnostic report generated yet.";
    public string DiagnosticReportSummaryText
    {
        get => diagnosticReportSummaryText;
        set => SetProperty(ref diagnosticReportSummaryText, value);
    }

    private string diagnosticReportCreatedAtText = "Created: -";
    public string DiagnosticReportCreatedAtText
    {
        get => diagnosticReportCreatedAtText;
        set => SetProperty(ref diagnosticReportCreatedAtText, value);
    }

    private string diagnosticReportButtonText = "Run Diagnosis";
    public string DiagnosticReportButtonText
    {
        get => diagnosticReportButtonText;
        set => SetProperty(ref diagnosticReportButtonText, value);
    }

    private string processCountText = "0 processes";
    public string ProcessCountText
    {
        get => processCountText;
        set => SetProperty(ref processCountText, value);
    }

    private string heaviestProcessText = "-";
    public string HeaviestProcessText
    {
        get => heaviestProcessText;
        set => SetProperty(ref heaviestProcessText, value);
    }

    private string processAlertText = "No data";
    public string ProcessAlertText
    {
        get => processAlertText;
        set => SetProperty(ref processAlertText, value);
    }

    private Brush processAlertBrush = Brushes.Gray;
    public Brush ProcessAlertBrush
    {
        get => processAlertBrush;
        set => SetProperty(ref processAlertBrush, value);
    }

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

    public ISeries[] CpuSeries { get; }

    public Axis[] CpuXAxes { get; }

    public Axis[] CpuYAxes { get; }

    public DrawMarginFrame CpuDrawMarginFrame { get; } = new()
    {
        Fill = new SolidColorPaint(new SKColor(21, 27, 46)),
        Stroke = new SolidColorPaint(new SKColor(36, 48, 79), 1)
    };

    public DashboardViewModel(DashboardFormatter formatter)
    {
        this.formatter = formatter;

        CpuSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = cpuValues,
                Name = "CPU %",
                GeometrySize = 0,
                LineSmoothness = 0.8,
                Stroke = new SolidColorPaint(new SKColor(56, 189, 248), 3),
                Fill = new SolidColorPaint(new SKColor(56, 189, 248, 35))
            }
        };

        CpuXAxes = new Axis[]
        {
            new Axis
            {
                IsVisible = false,
                SeparatorsPaint = null
            }
        };

        CpuYAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                LabelsPaint = new SolidColorPaint(new SKColor(143, 155, 184)),
                SeparatorsPaint = new SolidColorPaint(new SKColor(36, 48, 79), 1)
            }
        };

        SetInitialPreviewState();
        UpdateDiagnosticReport(null);
    }

    public void Update(MonitoringResult result)
    {
        SystemStats stats = result.Stats;

        CpuUsageText = $"{stats.CpuUsage:F1}%";
        CpuProgressValue = stats.CpuUsage;
        CpuProgressBrush = GetUsageBrush(stats.CpuUsage);

        double memoryUsagePercent = formatter.CalculateMemoryUsagePercent(stats);

        MemoryUsageText = formatter.FormatMemory(stats);
        MemoryProgressValue = memoryUsagePercent;
        MemoryProgressBrush = GetUsageBrush(memoryUsagePercent);

        UpdateApiStatus(result);
        UpdateDeviceInfo(result.Device);

        LastSyncText = result.LastSuccessfulSyncAt.HasValue
            ? $"Last Sync: {result.LastSuccessfulSyncAt.Value:HH:mm:ss}"
            : "Last Sync: -";

        UpdateDisks(stats);
        UpdateProcesses(stats);
        UpdateHistory(result.History);
        UpdateDiagnostics(result.Diagnostics);
        UpdateHealth(result.Health);
        AddCpuPoint(stats.CpuUsage);
    }

    public void UpdateDiagnosticReport(DiagnosticReportResponse? report)
    {
        ReportDetectedIssueItems.Clear();
        ReportRecommendationItems.Clear();

        if (report == null)
        {
            DiagnosticReportScoreText = "-- / 100";
            DiagnosticReportProgressValue = 0;
            DiagnosticReportStatusText = "NO REPORT";
            DiagnosticReportStatusBrush = Brushes.Gray;
            DiagnosticReportSummaryText = "No diagnostic report generated yet.";
            DiagnosticReportCreatedAtText = "Created: -";

            ReportDetectedIssueItems.Add("Run a diagnosis to generate a report.");
            ReportRecommendationItems.Add("No recommendations available yet.");

            return;
        }

        DiagnosticReportScoreText = $"{report.HealthScore} / 100";
        DiagnosticReportProgressValue = report.HealthScore;
        DiagnosticReportStatusText = report.Status;
        DiagnosticReportStatusBrush = GetHealthStatusBrush(report.Status);
        DiagnosticReportSummaryText = report.Summary;
        DiagnosticReportCreatedAtText = $"Created: {report.CreatedAt:HH:mm:ss}";

        foreach (string issue in report.DetectedIssues.Distinct())
        {
            ReportDetectedIssueItems.Add($"• {issue}");
        }

        foreach (string recommendation in report.Recommendations.Distinct())
        {
            ReportRecommendationItems.Add($"• {recommendation}");
        }
    }

    public void SetDiagnosticReportLoading(bool isLoading)
    {
        DiagnosticReportButtonText = isLoading ? "Running..." : "Run Diagnosis";
    }

    private void SetInitialPreviewState()
    {
        DiskItems.Add("Waiting for disk data...");
        DiskPreviewItems.Add("Waiting for disk data...");

        ProcessItems.Add("Waiting for process data...");
        ProcessPreviewItems.Add("Waiting for process data...");
        ProcessCountText = "0 processes";
        HeaviestProcessText = "-";
        ProcessAlertText = "No data";
        ProcessAlertBrush = Brushes.Gray;

        HistoryItems.Add("Waiting for history data...");
        HistoryPreviewItems.Add("Waiting for history data...");
        HistoryCountText = "0 entries";
        LatestHistoryTimeText = "-";
        LatestHistoryUsageText = "-";
        HistoryAlertText = "No data";
        HistoryAlertBrush = Brushes.Gray;
    }

    private void UpdateApiStatus(MonitoringResult result)
    {
        if (result.IsApiAvailable)
        {
            ApiStatusText = "● API: Connected";
            ApiStatusBrush = Brushes.LightGreen;
        }
        else
        {
            ApiStatusText = "● API: Offline";
            ApiStatusBrush = Brushes.IndianRed;
        }
    }

    private void UpdateDeviceInfo(DeviceRegistrationResponse? device)
    {
        if (device == null)
        {
            DeviceNameText = "Device: -";
            DeviceIdText = "ID: -";
            OperatingSystemText = "OS: -";
            return;
        }

        DeviceNameText = string.IsNullOrWhiteSpace(device.DeviceName)
            ? "Device: Unknown"
            : $"Device: {device.DeviceName}";

        DeviceIdText = $"ID: {device.Id}";

        OperatingSystemText = string.IsNullOrWhiteSpace(device.OperatingSystem)
            ? "OS: Unknown"
            : $"OS: {device.OperatingSystem}";
    }

    private void UpdateHistory(List<SystemStatsHistoryDto> history)
    {
        HistoryItems.Clear();
        HistoryPreviewItems.Clear();
        HistoryRows.Clear();

        List<SystemStatsHistoryDto> sortedHistory = history
            .OrderByDescending(item => item.CreatedAt)
            .Take(FullHistoryCount)
            .ToList();

        if (sortedHistory.Count == 0)
        {
            HistoryItems.Add("No history entries available yet.");
            HistoryPreviewItems.Add("No recent history available yet.");

            HistoryCountText = "0 entries";
            LatestHistoryTimeText = "-";
            LatestHistoryUsageText = "-";
            HistoryAlertText = "No data";
            HistoryAlertBrush = Brushes.Gray;

            return;
        }

        foreach (SystemStatsHistoryDto item in sortedHistory)
        {
            HistoryItems.Add(formatter.FormatHistory(item));
        }

        foreach (string item in HistoryItems.Take(HistoryPreviewCount))
        {
            HistoryPreviewItems.Add(item);
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

    private void UpdateDiagnostics(List<DiagnosticMessageDto> diagnostics)
    {
        DiagnosticItems.Clear();

        foreach (DiagnosticMessageDto diagnostic in diagnostics)
        {
            DiagnosticItems.Add(formatter.FormatDiagnostic(diagnostic));
        }
    }

    private void UpdateHealth(SystemHealthResponse? health)
    {
        HealthReasonItems.Clear();
        HealthRecommendationItems.Clear();

        if (health == null)
        {
            HealthScoreText = "-- / 100";
            HealthProgressValue = 0;
            HealthStatusText = "UNKNOWN";
            HealthStatusBrush = Brushes.Gray;
            HealthSummaryText = "No health data available.";

            HealthReasonItems.Add("System health data could not be loaded.");
            HealthRecommendationItems.Add("Check API connection.");

            return;
        }

        HealthScoreText = $"{health.Score} / 100";
        HealthProgressValue = health.Score;
        HealthStatusText = health.Status;
        HealthStatusBrush = GetHealthStatusBrush(health.Status);
        HealthSummaryText = GetHealthSummary(health.Status);

        AddHealthReasons(health);
        AddHealthRecommendations(health);
    }

    private void AddHealthReasons(SystemHealthResponse health)
    {
        if (health.Reasons.Count == 0)
        {
            HealthReasonItems.Add("No issues detected.");
            return;
        }

        foreach (string reason in health.Reasons.Distinct())
        {
            HealthReasonItems.Add($"• {reason}");
        }
    }

    private void AddHealthRecommendations(SystemHealthResponse health)
    {
        if (health.Recommendations.Count == 0)
        {
            HealthRecommendationItems.Add("No immediate action required.");
            return;
        }

        int index = 1;

        foreach (string recommendation in health.Recommendations.Distinct())
        {
            HealthRecommendationItems.Add($"{index}. {recommendation}");
            index++;
        }
    }

    private static string GetHealthSummary(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "HEALTHY" => "Your system is currently in good shape.",
            "WARNING" => "PCDoctor found performance pressure.",
            "CRITICAL" => "PCDoctor found serious performance issues.",
            _ => "System health could not be evaluated."
        };
    }

    private static Brush GetHealthStatusBrush(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "HEALTHY" => Brushes.LightGreen,
            "WARNING" => Brushes.Gold,
            "CRITICAL" => Brushes.IndianRed,
            _ => Brushes.Gray
        };
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

    private void UpdateProcesses(SystemStats stats)
    {
        ProcessItems.Clear();
        ProcessPreviewItems.Clear();
        ProcessRows.Clear();

        List<ProcessStats> sortedProcesses = stats.TopProcesses
            .OrderByDescending(process => process.MemoryUsageMB)
            .ToList();

        if (sortedProcesses.Count == 0)
        {
            ProcessItems.Add("No process data available yet.");
            ProcessPreviewItems.Add("No process data available yet.");

            ProcessCountText = "0 processes";
            HeaviestProcessText = "-";
            ProcessAlertText = "No data";
            ProcessAlertBrush = Brushes.Gray;

            return;
        }

        foreach (ProcessStats process in sortedProcesses)
        {
            ProcessItems.Add(formatter.FormatProcess(process));
        }

        foreach (string item in ProcessItems.Take(ProcessPreviewCount))
        {
            ProcessPreviewItems.Add(item);
        }

        double highestMemoryUsage = sortedProcesses.Max(process => process.MemoryUsageMB);

        for (int index = 0; index < sortedProcesses.Count; index++)
        {
            ProcessStats process = sortedProcesses[index];

            ProcessRows.Add(ProcessListItem.Create(
                position: index + 1,
                processName: process.ProcessName,
                processId: process.ProcessId,
                memoryUsageMb: process.MemoryUsageMB,
                highestMemoryUsageMb: highestMemoryUsage));
        }

        ProcessStats heaviestProcess = sortedProcesses[0];

        ProcessCountText = $"{sortedProcesses.Count} processes";
        HeaviestProcessText = $"{heaviestProcess.ProcessName} · {heaviestProcess.MemoryUsageMB:F0} MB";

        UpdateProcessAlert(heaviestProcess.MemoryUsageMB);
    }

    private void UpdateProcessAlert(double heaviestProcessMemoryMb)
    {
        if (heaviestProcessMemoryMb >= 2000)
        {
            ProcessAlertText = "Heavy";
            ProcessAlertBrush = Brushes.IndianRed;
            return;
        }

        if (heaviestProcessMemoryMb >= 1000)
        {
            ProcessAlertText = "High";
            ProcessAlertBrush = Brushes.Gold;
            return;
        }

        ProcessAlertText = "Normal";
        ProcessAlertBrush = Brushes.LightGreen;
    }

    private void UpdateDisks(SystemStats stats)
    {
        DiskItems.Clear();
        DiskPreviewItems.Clear();

        List<string> formattedDisks = stats.Disks
            .Select(formatter.FormatDisk)
            .ToList();

        if (formattedDisks.Count == 0)
        {
            DiskItems.Add("No disk data available yet.");
            DiskPreviewItems.Add("No disk data available yet.");
            return;
        }

        foreach (string item in formattedDisks)
        {
            DiskItems.Add(item);
        }

        foreach (string item in formattedDisks.Take(DiskPreviewCount))
        {
            DiskPreviewItems.Add(item);
        }
    }

    private void AddCpuPoint(double cpuUsage)
    {
        cpuValues.Add(cpuUsage);

        while (cpuValues.Count > MaxCpuChartPoints)
        {
            cpuValues.RemoveAt(0);
        }
    }
}

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