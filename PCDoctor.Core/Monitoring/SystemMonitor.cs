using PCDoctor.Models;

namespace PCDoctor.Core.Monitoring
{
    public class SystemMonitor
    {
        private readonly CpuMonitor cpuCounter;
        private readonly MemoryMonitor memoryMonitor;
        private readonly DiskMonitor diskMonitor;
        private readonly ProcessMonitor processMonitor;
        
        public SystemMonitor()
        {
            cpuCounter = new CpuMonitor();
            memoryMonitor = new MemoryMonitor();
            diskMonitor = new DiskMonitor();
            processMonitor = new ProcessMonitor();
        }

        public SystemStats GetStats()
        {
            float cpu = cpuCounter.GetCpuUsage();
            (float totalRam, float availableRam) = memoryMonitor.GetMemoryInfo();

            float usedRam = totalRam - availableRam;

            return new SystemStats
            {
                CpuUsage = cpu,
                TotalMemoryMB = totalRam,
                AvailableMemoryMB = availableRam,
                UsedMemoryMB = usedRam,
                Disks = diskMonitor.GetDiskStats(),
                TopProcesses = processMonitor.GetTopProcessByMemory()
            };
        }
    }
}
