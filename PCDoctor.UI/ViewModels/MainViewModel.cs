using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PCDoctor.Core.Models;
using PCDoctor.Models;
using PCDoctor.UI.Models;
using PCDoctor.UI.Services;

namespace PCDoctor.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private const int MaxCpuChartPoints = 30;
    private readonly DashboardFormatter formatter;
    private CancellationTokenSource? monitoringCancellationTokenSource;
    private readonly ObservableCollection<ObservablePoint> cpuPoints = new();
    public ObservableCollection<string> DiskItems { get; } = new();
    public ObservableCollection<string> ProcessItems { get; } = new();
    public ObservableCollection<string> HistoryItems { get; } = new();
    public ObservableCollection<string> DiagnosticItems { get; } = new();
    private readonly AppSettings settings;
    private readonly MonitoringService monitoringService;
    private string cpuUsageText = "0%";
    public string CpuUsageText
    {
        get => cpuUsageText;
        set
        {
            cpuUsageText = value;
            OnPropertyChanged();
        }
    }

    private double cpuProgressValue;
    public double CpuProgressValue
    {
        get => cpuProgressValue;
        set
        {
            cpuProgressValue = value;
            OnPropertyChanged();
        }
    }

    private string memoryUsageText = "0 GB / 0 GB";
    public string MemoryUsageText
    {
        get => memoryUsageText;
        set
        {
            memoryUsageText = value;
            OnPropertyChanged();
        }
    }

    private double memoryProgressValue;
    public double MemoryProgressValue
    {
        get => memoryProgressValue;
        set
        {
            memoryProgressValue = value;
            OnPropertyChanged();
        }
    }
    public ISeries[] CpuSeries { get; }

    public MainViewModel(AppSettings settings, MonitoringService monitoringService, DashboardFormatter formatter)
    {
        this.settings = settings;
        this.monitoringService = monitoringService;
        this.formatter = formatter;
        
        CpuSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = cpuPoints,
                Name = "CPU %",
                GeometrySize = 4
            }
        };
    }
    
    public async void StartMonitoring()
    {
        monitoringCancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = monitoringCancellationTokenSource.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                MonitoringResult result = await monitoringService.GetMonitoringResultAsync();

                UpdateDashboard(
                    result.Stats,
                    result.History,
                    result.Diagnostics
                );

                await Task.Delay(settings.RefreshIntervalSeconds * 1000, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Monitoring stopped
        }
    }
    
    private void UpdateDashboard(
        SystemStats stats,
        List<SystemStatsHistoryDto> history,
        List<DiagnosticMessageDto> diagnostics)
    {
        CpuUsageText = $"{stats.CpuUsage:F1}%";
        CpuProgressValue = stats.CpuUsage;

        MemoryUsageText = formatter.FormatMemory(stats);

        MemoryProgressValue = formatter.CalculateMemoryUsagePercent(stats);

        UpdateDisks(stats);
        UpdateProcesses(stats);
        UpdateHistory(history);
        AddCpuPoint(stats.CpuUsage);
        UpdateDiagnostics(diagnostics);
    }
    
    private void UpdateHistory(List<SystemStatsHistoryDto> history)
    {
        HistoryItems.Clear();

        foreach (SystemStatsHistoryDto item in history.Take(10))
        {
            HistoryItems.Add(formatter.FormatHistory(item));
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
    private void UpdateProcesses(SystemStats stats)
    {
        ProcessItems.Clear();

        foreach (ProcessStats process in stats.TopProcesses)
        {
            ProcessItems.Add(formatter.FormatProcess(process));
        }
    }
    private void UpdateDisks(SystemStats stats)
    {
        DiskItems.Clear();

        foreach (DiskStats disk in stats.Disks)
        {
            DiskItems.Add(formatter.FormatDisk(disk));
        }
    }
    public void AddCpuPoint(double cpuUsage)
    {
        cpuPoints.Add(new ObservablePoint(cpuPoints.Count, cpuUsage));

        if (cpuPoints.Count > MaxCpuChartPoints)
        {
            cpuPoints.RemoveAt(0);
        }
    }
    public void StopMonitoring()
    {
        monitoringCancellationTokenSource?.Cancel();
    }
}