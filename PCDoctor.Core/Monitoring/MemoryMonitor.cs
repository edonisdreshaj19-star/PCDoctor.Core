using System.Runtime.InteropServices;

namespace PCDoctor.Core.Monitoring;

public class MemoryMonitor
{
    public (float totalRam, float availableRam) GetMemoryInfo()
    {
        MEMORYSTATUSEX memStatus = new();

        if (GlobalMemoryStatusEx(memStatus))
        {
            float totalRamMB = memStatus.ullTotalPhys / (1024f * 1024f);
            float availableRamMB = memStatus.ullAvailPhys / (1024f * 1024f);

            return (totalRamMB, availableRamMB);
        }

        return (-1, -1);
    }
    
    [DllImport("kernel32.dll", SetLastError =  true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)] 
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }
}