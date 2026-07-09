using System.Collections.ObjectModel;
using System.Windows.Media;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.ViewModels;

public class ReportsViewModel : BaseViewModel
{
    public ObservableCollection<DiagnosticReportListItem> DiagnosticReportRows { get; } = new();

    private string diagnosticReportHistoryCountText = "0 reports";
    public string DiagnosticReportHistoryCountText
    {
        get => diagnosticReportHistoryCountText;
        set => SetProperty(ref diagnosticReportHistoryCountText, value);
    }

    private string latestDiagnosticReportTimeText = "-";
    public string LatestDiagnosticReportTimeText
    {
        get => latestDiagnosticReportTimeText;
        set => SetProperty(ref latestDiagnosticReportTimeText, value);
    }

    private string latestDiagnosticReportStatusText = "NO DATA";
    public string LatestDiagnosticReportStatusText
    {
        get => latestDiagnosticReportStatusText;
        set => SetProperty(ref latestDiagnosticReportStatusText, value);
    }

    private Brush latestDiagnosticReportStatusBrush = Brushes.Gray;
    public Brush LatestDiagnosticReportStatusBrush
    {
        get => latestDiagnosticReportStatusBrush;
        set => SetProperty(ref latestDiagnosticReportStatusBrush, value);
    }

    private string diagnosticReportHistoryButtonText = "Refresh History";
    public string DiagnosticReportHistoryButtonText
    {
        get => diagnosticReportHistoryButtonText;
        set => SetProperty(ref diagnosticReportHistoryButtonText, value);
    }

    public ReportsViewModel()
    {
        SetEmptyDiagnosticReportHistory();
    }

    public void UpdateDiagnosticReportHistory(List<DiagnosticReportResponse> reports)
    {
        DiagnosticReportRows.Clear();

        List<DiagnosticReportResponse> sortedReports = reports
            .OrderByDescending(report => report.CreatedAt)
            .ToList();

        if (sortedReports.Count == 0)
        {
            SetEmptyDiagnosticReportHistory();
            return;
        }

        DiagnosticReportHistoryCountText = $"{sortedReports.Count} reports";

        DiagnosticReportResponse latestReport = sortedReports[0];

        LatestDiagnosticReportTimeText = latestReport.CreatedAt.ToString("HH:mm:ss");
        LatestDiagnosticReportStatusText = latestReport.Status;
        LatestDiagnosticReportStatusBrush = GetReportStatusBrush(latestReport.Status);

        for (int index = 0; index < sortedReports.Count; index++)
        {
            DiagnosticReportRows.Add(DiagnosticReportListItem.Create(
                position: index + 1,
                report: sortedReports[index]));
        }
    }

    public void SetDiagnosticReportHistoryLoading(bool isLoading)
    {
        DiagnosticReportHistoryButtonText = isLoading ? "Refreshing..." : "Refresh History";
    }

    private void SetEmptyDiagnosticReportHistory()
    {
        DiagnosticReportRows.Clear();

        DiagnosticReportHistoryCountText = "0 reports";
        LatestDiagnosticReportTimeText = "-";
        LatestDiagnosticReportStatusText = "NO DATA";
        LatestDiagnosticReportStatusBrush = Brushes.Gray;

        DiagnosticReportRows.Add(DiagnosticReportListItem.CreateEmpty());
    }

    private static Brush GetReportStatusBrush(string status)
    {
        return status.ToUpperInvariant() switch
        {
            "HEALTHY" => Brushes.LightGreen,
            "WARNING" => Brushes.Gold,
            "CRITICAL" => Brushes.IndianRed,
            _ => Brushes.Gray
        };
    }
}