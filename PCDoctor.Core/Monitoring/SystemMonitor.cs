using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PCDoctor.Core.Models;

namespace PCDoctor.Core.Monitoring
{
    class SystemMonitor
    {
        private readonly CpuMonitor cpuCounter;
        private readonly MemoryMonitor memoryMonitor;
        private readonly DiskMonitor diskMonitor;
        
        public SystemMonitor()
        {
            cpuCounter = new CpuMonitor();
            memoryMonitor = new MemoryMonitor();
            diskMonitor = new DiskMonitor();
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
            };
        }
    }
}
