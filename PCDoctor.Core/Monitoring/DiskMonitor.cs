using PCDoctor.UI.Models;

namespace PCDoctor.Core.Monitoring;

public class DiskMonitor
{
    public List<DiskStats> GetDiskStats()
    {
        List<DiskStats> diskStatsList = new List<DiskStats>();

        DriveInfo[] drives = DriveInfo.GetDrives();

        foreach (DriveInfo drive in drives)
        {
            if ((!drive.IsReady))
            {
                continue;
            }

            double totalGb = ConvertBytesToDouble(drive.TotalSize);
            double freeGb = ConvertBytesToDouble(drive.AvailableFreeSpace);
            double usedGb = totalGb - freeGb;
            double usagePercentage = totalGb > 0 ? (usedGb / totalGb) * 100 : 0;

            diskStatsList.Add(new DiskStats
            {
                DriveName = drive.Name,
                TotalSpaceGB = totalGb,
                FreeSpaceGB = freeGb,
                UsedSpaceGB = usedGb,
                UsagePercentage = usagePercentage
            });
        }
        return diskStatsList;
    }
        
    private double ConvertBytesToDouble(long bytes) 
    { 
        return bytes / 1024d / 1024d / 1024d;
    }
}