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

            double totalGB = ConvertBytesToDouble(drive.TotalSize);
            double freeGB = ConvertBytesToDouble(drive.AvailableFreeSpace);
            double usedGB = totalGB - freeGB;
            double usagePercentage = totalGB > 0 ? (usedGB / totalGB) * 100 : 0;

            diskStatsList.Add(new DiskStats
            {
                DriveName = drive.Name,
                TotalSpaceGB = totalGB,
                FreeSpaceGB = freeGB,
                UsedSpaceGB = usedGB,
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