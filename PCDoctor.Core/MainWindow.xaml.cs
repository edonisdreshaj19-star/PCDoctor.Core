using System.Windows;
using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;

namespace PCDoctor.Core
{
    public partial class MainWindow : Window
    {
        
        private readonly SystemMonitor monitor;
        public MainWindow()
        {
            InitializeComponent();
            
            monitor = new SystemMonitor();
            StartMonitoring();
        }

        private async void StartMonitoring()
        {
            while (true)
            {
                SystemStats stats = monitor.GetStats();

                Dispatcher.Invoke(() =>
                    {
                        CpuUsageText.Text = $"{stats.CpuUsage:F1}%";
                        CpuProgressBar.Value = stats.CpuUsage;
                        
                        MemoryUsageText.Text = $"{stats.UsedMemoryMB} MB / {stats.TotalMemoryMB:F1} MB";

                        MemoryProgressBar.Value =
                            stats.TotalMemoryMB > 0
                                ? stats.UsedMemoryMB / stats.TotalMemoryMB * 100
                                : 0;
                        
                        DiskListBox.Items.Clear();

                        foreach (DiskStats disk in stats.Disks)
                        {
                            DiskListBox.Items.Add($"{disk.DriveName} {disk.UsedSpaceGB:F1} GB / {disk.TotalSpaceGB:F1} GB ({disk.UsagePercentage:F1}%)");
                        }
                    }
                );
                await Task.Delay(1000);
            }
        }
    }
}