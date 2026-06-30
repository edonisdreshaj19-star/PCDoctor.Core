namespace PCDoctor.UI.Models;

public class ProcessStats
{
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public double MemoryUsageMB { get; set; }
}