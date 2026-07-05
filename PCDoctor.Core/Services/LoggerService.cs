using Serilog;

namespace PCDoctor.Core.Services;

public class LoggerService
{
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                "logs/pcdoctor-log.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void Close()
    {
        Log.CloseAndFlush();
    }
}