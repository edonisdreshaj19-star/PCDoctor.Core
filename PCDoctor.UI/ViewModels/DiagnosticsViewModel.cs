using System.Collections.ObjectModel;
using System.Windows.Media;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.ViewModels;

public class DiagnosticsViewModel : BaseViewModel
{
    public ObservableCollection<string> ReportDetectedIssueItems { get; } = new();
    public ObservableCollection<string> ReportRecommendationItems { get; } = new();

    private string diagnosticReportScoreText = "-- / 100";
    public string DiagnosticReportScoreText
    {
        get => diagnosticReportScoreText;
        set => SetProperty(ref diagnosticReportScoreText, value);
    }

    private double diagnosticReportProgressValue;
    public double DiagnosticReportProgressValue
    {
        get => diagnosticReportProgressValue;
        set => SetProperty(ref diagnosticReportProgressValue, value);
    }

    private string diagnosticReportStatusText = "NO REPORT";
    public string DiagnosticReportStatusText
    {
        get => diagnosticReportStatusText;
        set => SetProperty(ref diagnosticReportStatusText, value);
    }

    private Brush diagnosticReportStatusBrush = Brushes.Gray;
    public Brush DiagnosticReportStatusBrush
    {
        get => diagnosticReportStatusBrush;
        set => SetProperty(ref diagnosticReportStatusBrush, value);
    }

    private string diagnosticReportSummaryText = "No diagnostic report generated yet.";
    public string DiagnosticReportSummaryText
    {
        get => diagnosticReportSummaryText;
        set => SetProperty(ref diagnosticReportSummaryText, value);
    }

    private string diagnosticReportCreatedAtText = "Created: -";
    public string DiagnosticReportCreatedAtText
    {
        get => diagnosticReportCreatedAtText;
        set => SetProperty(ref diagnosticReportCreatedAtText, value);
    }

    private string diagnosticReportButtonText = "Run Diagnosis";
    public string DiagnosticReportButtonText
    {
        get => diagnosticReportButtonText;
        set => SetProperty(ref diagnosticReportButtonText, value);
    }

    public DiagnosticsViewModel()
    {
        UpdateDiagnosticReport(null);
    }

    public void UpdateDiagnosticReport(DiagnosticReportResponse? report)
    {
        ReportDetectedIssueItems.Clear();
        ReportRecommendationItems.Clear();

        if (report == null)
        {
            DiagnosticReportScoreText = "-- / 100";
            DiagnosticReportProgressValue = 0;
            DiagnosticReportStatusText = "NO REPORT";
            DiagnosticReportStatusBrush = Brushes.Gray;
            DiagnosticReportSummaryText = "No diagnostic report generated yet.";
            DiagnosticReportCreatedAtText = "Created: -";

            ReportDetectedIssueItems.Add("Run a diagnosis to generate a report.");
            ReportRecommendationItems.Add("No recommendations available yet.");

            return;
        }

        DiagnosticReportScoreText = $"{report.HealthScore} / 100";
        DiagnosticReportProgressValue = Math.Clamp(report.HealthScore, 0, 100);
        DiagnosticReportStatusText = report.Status;
        DiagnosticReportStatusBrush = GetStatusBrush(report.Status);
        DiagnosticReportSummaryText = string.IsNullOrWhiteSpace(report.Summary)
            ? "No summary available."
            : report.Summary;
        DiagnosticReportCreatedAtText = $"Created: {report.CreatedAt:HH:mm:ss}";

        foreach (string issue in report.DetectedIssues.Distinct())
        {
            ReportDetectedIssueItems.Add($"• {issue}");
        }

        foreach (string recommendation in report.Recommendations.Distinct())
        {
            ReportRecommendationItems.Add($"• {recommendation}");
        }

        if (ReportDetectedIssueItems.Count == 0)
        {
            ReportDetectedIssueItems.Add("No issues detected.");
        }

        if (ReportRecommendationItems.Count == 0)
        {
            ReportRecommendationItems.Add("No recommendations available.");
        }
    }

    public void SetDiagnosticReportLoading(bool isLoading)
    {
        DiagnosticReportButtonText = isLoading ? "Running..." : "Run Diagnosis";
    }

    private static Brush GetStatusBrush(string status)
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