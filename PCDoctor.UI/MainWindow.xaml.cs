using System.Windows;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI
{
    public partial class MainWindow : Window
    {
        
        private readonly SystemMonitor monitor;
        private readonly ApiService apiService;
        public MainWindow()
        {
            InitializeComponent();
            
            monitor = new SystemMonitor();
            apiService = new ApiService();
            
            StartMonitoring();
        }

        private async void StartMonitoring()
        {
            while (true)
            {
                SystemStats stats = monitor.GetStats();
                
                await apiService.SendSystemStatsAsync(stats);
                
                Dispatcher.Invoke(() =>
                    {
                        CpuUsageText.Text = $"{stats.CpuUsage:F1}%";
                        CpuProgressBar.Value = stats.CpuUsage;
                        
                        MemoryUsageText.Text = $"{stats.UsedMemoryMB / 1024:F2} GB / {stats.TotalMemoryMB / 1024:F2} GB";

                        MemoryProgressBar.Value =
                            stats.TotalMemoryMB > 0
                                ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
                                : 0;
                        
                        DiskListBox.Items.Clear();

                        foreach (DiskStats disk in stats.Disks)
                        {
                            DiskListBox.Items.Add($"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)");
                        }
                        
                        ProcessListBox.Items.Clear();

                        foreach (ProcessStats process in stats.TopProcesses)
                        {
                            ProcessListBox.Items.Add(
                                $"{process.ProcessName} ({process.ProcessId}) - {process.MemoryUsageMB:F1} MB"
                            );
                        }
                    }
                    
                );
                await Task.Delay(1000);
            }
        }
    }
}