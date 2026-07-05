using System.Collections.ObjectModel;
using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI;

public partial class MainWindow : Window
{
    private const int MaxCpuChartPoints = 30;

    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;
    private readonly ObservableCollection<ObservablePoint> cpuPoints = new();
    private readonly SettingsService settingsService;
    
    
    private DateTime lastApiSendTime = DateTime.MinValue;

    public ISeries[] CpuSeries { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        
        settingsService = new SettingsService();
        settings = settingsService.LoadSettings();

        monitor = new SystemMonitor();
        apiService = new ApiService(settings);

        InitializeChart();
        DataContext = this;

        StartMonitoring();
    }

    private void InitializeChart()
    {
        CpuSeries =
        [
            new LineSeries<ObservablePoint>
            {
                Values = cpuPoints,
                Name = "CPU %",
                GeometrySize = 4
            }
        ];
    }

    private async void StartMonitoring()
    {
        while (true)
        {
            SystemStats stats = monitor.GetStats();
            List<SystemStatsHistoryDto> history = await apiService.GetHistoryAsync();
            List<DiagnosticMessageDto> diagnostics = await apiService.GetDiagnosticsAsync();
            
            Dispatcher.Invoke(() => UpdateUi(stats, history, diagnostics));

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

    private void UpdateUi(SystemStats stats, List<SystemStatsHistoryDto> history, List<DiagnosticMessageDto> diagnostics)
    {
        UpdateCpu(stats);
        UpdateMemory(stats);
        UpdateDisks(stats);
        UpdateProcesses(stats);
        UpdateHistory(history);
        UpdateCpuChart(stats);
        UpdateDiagnostics(diagnostics);
    }

    private void UpdateCpu(SystemStats stats)
    {
        CpuUsageText.Text = $"{stats.CpuUsage:F1}%";
        CpuProgressBar.Value = stats.CpuUsage;
    }

    private void UpdateMemory(SystemStats stats)
    {
        MemoryUsageText.Text =
            $"{stats.UsedMemoryMB / 1024:F2} GB / {stats.TotalMemoryMB / 1024:F2} GB";

        MemoryProgressBar.Value =
            stats.TotalMemoryMB > 0
                ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
                : 0;
    }

    private void UpdateDisks(SystemStats stats)
    {
        DiskListBox.Items.Clear();

        foreach (DiskStats disk in stats.Disks)
        {
            DiskListBox.Items.Add(
                $"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)"
            );
        }
    }

    private void UpdateProcesses(SystemStats stats)
    {
        ProcessListBox.Items.Clear();

        foreach (ProcessStats process in stats.TopProcesses)
        {
            ProcessListBox.Items.Add(
                $"{process.ProcessName} ({process.ProcessId}) - {process.MemoryUsageMB:F1} MB"
            );
        }
    }

    private void UpdateHistory(List<SystemStatsHistoryDto> history)
    {
        HistoryListBox.Items.Clear();

        foreach (SystemStatsHistoryDto item in history.Take(10))
        {
            double memoryPercent =
                item.TotalMemoryMb > 0
                    ? item.UsedMemoryMb / item.TotalMemoryMb * 100
                    : 0;

            HistoryListBox.Items.Add(
                $"{item.CreatedAt:HH:mm:ss} | CPU {item.CpuUsage:F1}% | RAM {memoryPercent:F1}%"
            );
        }
    }

    private void UpdateCpuChart(SystemStats stats)
    {
        cpuPoints.Add(new ObservablePoint(cpuPoints.Count, stats.CpuUsage));

        if (cpuPoints.Count > MaxCpuChartPoints)
        {
            cpuPoints.RemoveAt(0);
        }
    }
    
    private void UpdateDiagnostics(List<DiagnosticMessageDto> diagnostics)
    {
        DiagnosticsListBox.Items.Clear();

        foreach (DiagnosticMessageDto diagnostic in diagnostics)
        {
            DiagnosticsListBox.Items.Add(
                $"{diagnostic.Level}: {diagnostic.Message}"
            );
        }
    }
    
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new(settings, settingsService)
        {
            Owner = this
        };

        settingsWindow.ShowDialog();
    }
}