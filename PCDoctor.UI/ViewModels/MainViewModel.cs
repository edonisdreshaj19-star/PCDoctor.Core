using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

namespace PCDoctor.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private const int MaxCpuChartPoints = 30;
    
    private readonly ObservableCollection<ObservablePoint> cpuPoints = new();
    public ObservableCollection<string> DiskItems { get; } = new();
    public ObservableCollection<string> ProcessItems { get; } = new();
    public ObservableCollection<string> HistoryItems { get; } = new();
    public ObservableCollection<string> DiagnosticItems { get; } = new();
    
    private string cpuUsageText = "0%";
    public string CpuUsageText
    {
        get => cpuUsageText;
        set
        {
            cpuUsageText = value;
            OnPropertyChanged();
        }
    }

    private double cpuProgressValue;
    public double CpuProgressValue
    {
        get => cpuProgressValue;
        set
        {
            cpuProgressValue = value;
            OnPropertyChanged();
        }
    }

    private string memoryUsageText = "0 GB / 0 GB";
    public string MemoryUsageText
    {
        get => memoryUsageText;
        set
        {
            memoryUsageText = value;
            OnPropertyChanged();
        }
    }

    private double memoryProgressValue;
    public double MemoryProgressValue
    {
        get => memoryProgressValue;
        set
        {
            memoryProgressValue = value;
            OnPropertyChanged();
        }
    }
    public ISeries[] CpuSeries { get; }

    public MainViewModel()
    {
        CpuSeries = new ISeries[]
        {
            new LineSeries<ObservablePoint>
            {
                Values = cpuPoints,
                Name = "CPU %",
                GeometrySize = 4
            }
        };
    }
    
    public void AddCpuPoint(double cpuUsage)
    {
        cpuPoints.Add(new ObservablePoint(cpuPoints.Count, cpuUsage));

        if (cpuPoints.Count > MaxCpuChartPoints)
        {
            cpuPoints.RemoveAt(0);
        }
    }
}