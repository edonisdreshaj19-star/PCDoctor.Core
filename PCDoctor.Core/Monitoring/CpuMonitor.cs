namespace PCDoctor.Core.Monitoring;
using System.Diagnostics;

public class CpuMonitor
{
    private readonly PerformanceCounter cpuCounter;
    
    public CpuMonitor()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        
        cpuCounter.NextValue();
        Thread.Sleep(100);
    }
    
    public float GetCpuUsage()
    {
        try
        {
            return cpuCounter.NextValue();
        }
        catch
        {
            return -1;
        }
    }
}