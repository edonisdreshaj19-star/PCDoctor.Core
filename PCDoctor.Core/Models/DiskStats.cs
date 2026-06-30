namespace PCDoctor.UI.Models;

public class DiskStats
{
    public string DriveName { get; set; }
    public double TotalSpaceGB { get; set; }
    public double FreeSpaceGB { get; set; }
    public double UsedSpaceGB { get; set; }
    public double UsagePercentage { get; set; }
}