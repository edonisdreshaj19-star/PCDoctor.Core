using System.Windows.Media;
using PCDoctor.UI.Models;

namespace PCDoctor.UI.ViewModels;

public class DiagnosticReportListItem
{
    public string PositionText { get; }
    public string CreatedAtText { get; }
    public string HealthScoreText { get; }
    public double HealthScoreBarValue { get; }
    public string StatusText { get; }
    public Brush StatusBrush { get; }
    public string SummaryText { get; }

    private DiagnosticReportListItem(
        string positionText,
        string createdAtText,
        string healthScoreText,
        double healthScoreBarValue,
        string statusText,
        Brush statusBrush,
        string summaryText)
    {
        PositionText = positionText;
        CreatedAtText = createdAtText;
        HealthScoreText = healthScoreText;
        HealthScoreBarValue = healthScoreBarValue;
        StatusText = statusText;
        StatusBrush = statusBrush;
        SummaryText = summaryText;
    }

    public static DiagnosticReportListItem Create(int position, DiagnosticReportResponse report)
    {
        return new DiagnosticReportListItem(
            positionText: $"#{position}",
            createdAtText: report.CreatedAt.ToString("HH:mm:ss"),
            healthScoreText: $"{report.HealthScore} / 100",
            healthScoreBarValue: Math.Clamp(report.HealthScore, 0, 100),
            statusText: report.Status,
            statusBrush: GetStatusBrush(report.Status),
            summaryText: string.IsNullOrWhiteSpace(report.Summary)
                ? "No summary available."
                : report.Summary);
    }

    public static DiagnosticReportListItem CreateEmpty()
    {
        return new DiagnosticReportListItem(
            positionText: "-",
            createdAtText: "-",
            healthScoreText: "-",
            healthScoreBarValue: 0,
            statusText: "NO DATA",
            statusBrush: Brushes.Gray,
            summaryText: "No diagnostic reports available yet. Run a diagnosis to create the first report.");
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