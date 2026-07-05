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
using PCDoctor.UI.ViewModels;

namespace PCDoctor.UI;

public partial class MainWindow : Window
{
    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;
    private readonly SettingsService settingsService;
    private readonly MainViewModel viewModel;
    
    
    private DateTime lastApiSendTime = DateTime.MinValue;

    public MainWindow()
    {
        InitializeComponent();
        
        
        settingsService = new SettingsService();
        settings = settingsService.LoadSettings();

        monitor = new SystemMonitor();
        apiService = new ApiService(settings);

        viewModel = new MainViewModel();
        DataContext = viewModel;
        
        StartMonitoring();
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
        viewModel.CpuUsageText = $"{stats.CpuUsage:F1}%";
        viewModel.CpuProgressValue = stats.CpuUsage;
    }

    private void UpdateMemory(SystemStats stats)
    {
        viewModel.MemoryUsageText =
            $"{stats.UsedMemoryMB / 1024:F2} GB / {stats.TotalMemoryMB / 1024:F2} GB";

        viewModel.MemoryProgressValue =
            stats.TotalMemoryMB > 0
                ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
                : 0;
    }

    private void UpdateDisks(SystemStats stats)
    {
        viewModel.DiskItems.Clear();

        foreach (DiskStats disk in stats.Disks)
        {
            viewModel.DiskItems.Add(
                $"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)"
            );
        }
    }

    private void UpdateProcesses(SystemStats stats)
    {
        viewModel.ProcessItems.Clear();

        foreach (ProcessStats process in stats.TopProcesses)
        {
            viewModel.ProcessItems.Add(
                $"{process.ProcessName} ({process.ProcessId}) - {process.MemoryUsageMB:F1} MB"
            );
        }
    }

    private void UpdateHistory(List<SystemStatsHistoryDto> history)
    {
        viewModel.HistoryItems.Clear();

        foreach (SystemStatsHistoryDto item in history.Take(10))
        {
            double memoryPercent =
                item.TotalMemoryMb > 0
                    ? item.UsedMemoryMb / item.TotalMemoryMb * 100
                    : 0;

            viewModel.HistoryItems.Add(
                $"{item.CreatedAt:HH:mm:ss} | CPU {item.CpuUsage:F1}% | RAM {memoryPercent:F1}%"
            );
        }
    }

    private void UpdateCpuChart(SystemStats stats)
    {
        viewModel.AddCpuPoint(stats.CpuUsage);
    }
    
    private void UpdateDiagnostics(List<DiagnosticMessageDto> diagnostics)
    {
        viewModel.DiagnosticItems.Clear();

        foreach (DiagnosticMessageDto diagnostic in diagnostics)
        {
            viewModel.DiagnosticItems.Add(
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