using PCDoctor.UI.Models;

namespace PCDoctor.Models
{
    public class SystemStats
    {
        public float CpuUsage { get; set; }
        public float TotalMemoryMB { get; set; }
        public float AvailableMemoryMB { get; set; }
        public float UsedMemoryMB { get; set; }
        public List<DiskStats> Disks { get; set; } = new();
        public List<ProcessStats> TopProcesses { get; set; } = new();
    }
}
