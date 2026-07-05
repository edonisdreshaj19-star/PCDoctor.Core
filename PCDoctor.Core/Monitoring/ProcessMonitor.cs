using System.Diagnostics;
using PCDoctor.UI.Models;
using Serilog;

namespace PCDoctor.Core.Monitoring;

public class ProcessMonitor
{
    public List<ProcessStats> GetTopProcessByMemory(int count = 5)
    {
        return Process.GetProcesses()
            .Select(process => CreateProcessesStats(process))
            .Where(process => process != null)
            .OrderByDescending(process => process!.MemoryUsageMB)
            .Take(count)
            .ToList()!;
    }

    private ProcessStats? CreateProcessesStats(Process process)
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
            Log.Debug(
                e,
                "Could not read process information for process {ProcessName} with ID {ProcessId}.",
                SafeGetProcessName(process),
                SafeGetProcessId(process));
            return null;
        }
    }
    
    private static string SafeGetProcessName(Process process)
    {
        try
        {
            return process.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }

    private static int SafeGetProcessId(Process process)
    {
        try
        {
            return process.Id;
        }
        catch
        {
            return -1;
        }
    }
}