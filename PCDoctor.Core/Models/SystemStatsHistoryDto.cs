namespace PCDoctor.Core.Models;

public class SystemStatsHistoryDto
{
    public long Id { get; set; }
    public double CpuUsage { get; set; }
    public double UsedMemoryMb { get; set; }
    public double TotalMemoryMb { get; set; }
    public DateTime CreatedAt { get; set; }
}