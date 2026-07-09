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
    public ObservableCollection<string> HistoryItems { get; } = new();
    public ObservableCollection<string> HistoryPreviewItems { get; } = new();
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

        HistoryItems.Add("Waiting for history data...");
        HistoryPreviewItems.Add("Waiting for history data...");
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

        List<string> formattedHistory = history
            .Take(FullHistoryCount)
            .Select(formatter.FormatHistory)
            .ToList();

        if (formattedHistory.Count == 0)
        {
            HistoryItems.Add("No history entries available yet.");
            HistoryPreviewItems.Add("No recent history available yet.");
            return;
        }

        foreach (string item in formattedHistory)
        {
            HistoryItems.Add(item);
        }

        foreach (string item in formattedHistory.Take(HistoryPreviewCount))
        {
            HistoryPreviewItems.Add(item);
        }
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

        List<string> formattedProcesses = stats.TopProcesses
            .Select(formatter.FormatProcess)
            .ToList();

        if (formattedProcesses.Count == 0)
        {
            ProcessItems.Add("No process data available yet.");
            ProcessPreviewItems.Add("No process data available yet.");
            return;
        }

        foreach (string item in formattedProcesses)
        {
            ProcessItems.Add(item);
        }

        foreach (string item in formattedProcesses.Take(ProcessPreviewCount))
        {
            ProcessPreviewItems.Add(item);
        }
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