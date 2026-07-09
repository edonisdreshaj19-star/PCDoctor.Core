using System.Collections.ObjectModel;
using System.Windows.Media;
using PCDoctor.Core.Models;
using PCDoctor.Models;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.ViewModels;

public class ProcessesViewModel : BaseViewModel
{
    public ObservableCollection<ProcessListItem> ProcessRows { get; } = new();

    private string processCountText = "0 processes";
    public string ProcessCountText
    {
        get => processCountText;
        set => SetProperty(ref processCountText, value);
    }

    private string heaviestProcessText = "-";
    public string HeaviestProcessText
    {
        get => heaviestProcessText;
        set => SetProperty(ref heaviestProcessText, value);
    }

    private string processAlertText = "No data";
    public string ProcessAlertText
    {
        get => processAlertText;
        set => SetProperty(ref processAlertText, value);
    }

    private Brush processAlertBrush = Brushes.Gray;
    public Brush ProcessAlertBrush
    {
        get => processAlertBrush;
        set => SetProperty(ref processAlertBrush, value);
    }

    public ProcessesViewModel()
    {
        SetEmptyProcessState();
    }

    public void Update(SystemStats stats)
    {
        ProcessRows.Clear();

        List<ProcessStats> sortedProcesses = stats.TopProcesses
            .OrderByDescending(process => process.MemoryUsageMB)
            .ToList();

        if (sortedProcesses.Count == 0)
        {
            SetEmptyProcessState();
            return;
        }

        double highestMemoryUsage = sortedProcesses.Max(process => process.MemoryUsageMB);

        for (int index = 0; index < sortedProcesses.Count; index++)
        {
            ProcessStats process = sortedProcesses[index];

            ProcessRows.Add(ProcessListItem.Create(
                position: index + 1,
                processName: process.ProcessName,
                processId: process.ProcessId,
                memoryUsageMb: process.MemoryUsageMB,
                highestMemoryUsageMb: highestMemoryUsage));
        }

        ProcessStats heaviestProcess = sortedProcesses[0];

        ProcessCountText = $"{sortedProcesses.Count} processes";
        HeaviestProcessText = $"{heaviestProcess.ProcessName} · {heaviestProcess.MemoryUsageMB:F0} MB";

        UpdateProcessAlert(heaviestProcess.MemoryUsageMB);
    }

    private void SetEmptyProcessState()
    {
        ProcessRows.Clear();

        ProcessCountText = "0 processes";
        HeaviestProcessText = "-";
        ProcessAlertText = "No data";
        ProcessAlertBrush = Brushes.Gray;
    }

    private void UpdateProcessAlert(double heaviestProcessMemoryMb)
    {
        if (heaviestProcessMemoryMb >= 2000)
        {
            ProcessAlertText = "Heavy";
            ProcessAlertBrush = Brushes.IndianRed;
            return;
        }

        if (heaviestProcessMemoryMb >= 1000)
        {
            ProcessAlertText = "High";
            ProcessAlertBrush = Brushes.Gold;
            return;
        }

        ProcessAlertText = "Normal";
        ProcessAlertBrush = Brushes.LightGreen;
    }
}