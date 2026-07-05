using Serilog;
using System.Diagnostics;

namespace PCDoctor.Core.Monitoring;

public class CpuMonitor
{
    private readonly PerformanceCounter _cpuCounter;
    
    public CpuMonitor()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        
        _cpuCounter.NextValue();
        Thread.Sleep(100);
    }
    
    public float GetCpuUsage()
    {
        try
        {
            return _cpuCounter.NextValue();
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to read CPU usage.");
            return -1;
        }
    }
}