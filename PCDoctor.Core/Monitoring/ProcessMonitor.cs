using System.Diagnostics;
using PCDoctor.UI.Models;

namespace PCDoctor.Core.Monitoring;

public class ProcessMonitor
{
    public List<ProcessStats> GetTopProcessByMemory(int count = 5)
    {
        return Process.GetProcesses()
            .Select(process => CreateProcessStats(process))
            .Where(process => process != null)
            .OrderByDescending(process => process!.MemoryUsageMB)
            .Take(count)
            .ToList()!;
    }

    private ProcessStats? CreateProcessStats(Process process)
    {
        try
        {
            return new ProcessStats
            {
                ProcessName = process.ProcessName,
                ProcessId = process.Id,
                MemoryUsageMB = process.WorkingSet64 / 1024d / 1024d
            };
        }
        catch (Exception e)
        {
            return null;
        }
    }
}