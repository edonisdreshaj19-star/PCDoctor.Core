using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private const int MaxCpuChartPoints = 30;
    
    private readonly ObservableCollection<ObservablePoint> cpuPoints = new();
    public ObservableCollection<string> DiskItems { get; } = new();
    public ObservableCollection<string> ProcessItems { get; } = new();
    public ObservableCollection<string> HistoryItems { get; } = new();
    public ObservableCollection<string> DiagnosticItems { get; } = new();
    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;

    private DateTime lastApiSendTime = DateTime.MinValue;
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

    public MainViewModel(AppSettings settings, SystemMonitor monitor, ApiService apiService)
    {
        this.settings = settings;
        this.monitor = monitor;
        this.apiService = apiService;
        
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
        while (true)
        {
            SystemStats stats = monitor.GetStats();

            List<SystemStatsHistoryDto> history = await apiService.GetHistoryAsync();
            List<DiagnosticMessageDto> diagnostics = await apiService.GetDiagnosticsAsync();

            UpdateDashboard(stats, history, diagnostics);

            await SendStatsIfNeededAsync(stats);

            await Task.Delay(settings.RefreshIntervalSeconds * 1000);
        }
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
    
    private void UpdateDashboard(
        SystemStats stats,
        List<SystemStatsHistoryDto> history,
        List<DiagnosticMessageDto> diagnostics)
    {
        CpuUsageText = $"{stats.CpuUsage:F1}%";
        CpuProgressValue = stats.CpuUsage;

        MemoryUsageText =
            $"{stats.UsedMemoryMB / 1024:F2} GB / {stats.TotalMemoryMB / 1024:F2} GB";

        MemoryProgressValue =
            stats.TotalMemoryMB > 0
                ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
                : 0;

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
            double memoryPercent =
                item.TotalMemoryMb > 0
                    ? item.UsedMemoryMb / item.TotalMemoryMb * 100
                    : 0;

            HistoryItems.Add(
                $"{item.CreatedAt:HH:mm:ss} | CPU {item.CpuUsage:F1}% | RAM {memoryPercent:F1}%"
            );
        }
    }
    private void UpdateDiagnostics(List<DiagnosticMessageDto> diagnostics)
    {
        DiagnosticItems.Clear();

        foreach (DiagnosticMessageDto diagnostic in diagnostics)
        {
            DiagnosticItems.Add(
                $"{diagnostic.Level}: {diagnostic.Message}"
            );
        }
    }
    private void UpdateProcesses(SystemStats stats)
    {
        ProcessItems.Clear();

        foreach (ProcessStats process in stats.TopProcesses)
        {
            ProcessItems.Add(
                $"{process.ProcessName} ({process.ProcessId}) - {process.MemoryUsageMB:F1} MB"
            );
        }
    }
    private void UpdateDisks(SystemStats stats)
    {
        DiskItems.Clear();

        foreach (DiskStats disk in stats.Disks)
        {
            DiskItems.Add(
                $"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)"
            );
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
}