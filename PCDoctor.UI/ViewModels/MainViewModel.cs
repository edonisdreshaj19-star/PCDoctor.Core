﻿using System.Windows.Input;
using PCDoctor.Core.Models;
using PCDoctor.Core.Services;
using PCDoctor.UI.Commands;
using PCDoctor.UI.Models;
using PCDoctor.UI.Services;

namespace PCDoctor.UI.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly AppSettings settings;
    private readonly MonitoringService monitoringService;
    private readonly ApiService apiService;

    private CancellationTokenSource? monitoringCancellationTokenSource;

    private string selectedPage = "Overview";

    public DashboardViewModel Dashboard { get; }
    public ProcessesViewModel Processes { get; }
    public HistoryViewModel History { get; }
    public DiagnosticsViewModel Diagnostics { get; }
    public ReportsViewModel Reports { get; }
    public SettingsViewModel Settings { get; }

    public ICommand GenerateDiagnosticReportCommand { get; }
    public ICommand RefreshDiagnosticReportsCommand { get; }
    public ICommand OpenReportsCommand { get; }
    public ICommand OpenDiagnosticsCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    public MainViewModel(
        AppSettings settings,
        SettingsService settingsService,
        MonitoringService monitoringService,
        ApiService apiService,
        DashboardFormatter formatter)
    {
        this.settings = settings;
        this.monitoringService = monitoringService;
        this.apiService = apiService;

        Dashboard = new DashboardViewModel(formatter);
        Processes = new ProcessesViewModel();
        History = new HistoryViewModel();
        Diagnostics = new DiagnosticsViewModel();
        Reports = new ReportsViewModel();
        Settings = new SettingsViewModel(settings, settingsService, apiService);

        GenerateDiagnosticReportCommand = new RelayCommand(() => _ = GenerateDiagnosticReportAsync());
        RefreshDiagnosticReportsCommand = new RelayCommand(() => _ = RefreshDiagnosticReportsAsync());
        OpenReportsCommand = new RelayCommand(() => SelectPage("Reports"));
        OpenDiagnosticsCommand = new RelayCommand(() => SelectPage("Diagnostics"));
        OpenSettingsCommand = new RelayCommand(() => SelectPage("Settings"));
    }

    public string SelectedPage
    {
        get => selectedPage;
        private set
        {
            if (!SetProperty(ref selectedPage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(PageSubtitle));

            OnPropertyChanged(nameof(IsOverviewSelected));
            OnPropertyChanged(nameof(IsDiagnosticsSelected));
            OnPropertyChanged(nameof(IsReportsSelected));
            OnPropertyChanged(nameof(IsHistorySelected));
            OnPropertyChanged(nameof(IsProcessesSelected));
            OnPropertyChanged(nameof(IsSettingsSelected));
        }
    }

    public string PageTitle => SelectedPage switch
    {
        "Overview" => "Overview",
        "Diagnostics" => "Diagnostics",
        "Reports" => "Reports",
        "History" => "History",
        "Processes" => "Processes",
        "Settings" => "Settings",
        _ => "PCDoctor"
    };

    public string PageSubtitle => SelectedPage switch
    {
        "Overview" => "Important system information at a glance.",
        "Diagnostics" => "Detailed diagnostic report and recommendations.",
        "Reports" => "Previously generated diagnostic reports.",
        "History" => "Previously collected system statistics.",
        "Processes" => "Running processes and memory-heavy applications.",
        "Settings" => "Application and API configuration.",
        _ => "System Monitoring & Diagnostics"
    };

    public bool IsOverviewSelected
    {
        get => SelectedPage == "Overview";
        set
        {
            if (value)
            {
                SelectPage("Overview");
            }
        }
    }

    public bool IsDiagnosticsSelected
    {
        get => SelectedPage == "Diagnostics";
        set
        {
            if (value)
            {
                SelectPage("Diagnostics");
            }
        }
    }

    public bool IsReportsSelected
    {
        get => SelectedPage == "Reports";
        set
        {
            if (value)
            {
                SelectPage("Reports");
            }
        }
    }

    public bool IsHistorySelected
    {
        get => SelectedPage == "History";
        set
        {
            if (value)
            {
                SelectPage("History");
            }
        }
    }

    public bool IsProcessesSelected
    {
        get => SelectedPage == "Processes";
        set
        {
            if (value)
            {
                SelectPage("Processes");
            }
        }
    }

    public bool IsSettingsSelected
    {
        get => SelectedPage == "Settings";
        set
        {
            if (value)
            {
                SelectPage("Settings");
            }
        }
    }

    public async void StartMonitoring()
    {
        if (monitoringCancellationTokenSource != null)
        {
            return;
        }

        monitoringCancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = monitoringCancellationTokenSource.Token;

        try
        {
            await RefreshDiagnosticReportsAsync();

            while (!token.IsCancellationRequested)
            {
                var result = await monitoringService.GetMonitoringResultAsync();

                Dashboard.Update(result);
                Processes.Update(result.Stats);
                History.Update(result.History);
                Settings.UpdateRuntimeStatus();

                await Task.Delay(settings.RefreshIntervalSeconds * 1000, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Monitoring stopped
        }
        finally
        {
            monitoringCancellationTokenSource?.Dispose();
            monitoringCancellationTokenSource = null;
        }
    }

    public void StopMonitoring()
    {
        monitoringCancellationTokenSource?.Cancel();
    }

    private void SelectPage(string page)
    {
        SelectedPage = page;
    }

    private async Task GenerateDiagnosticReportAsync()
    {
        Diagnostics.SetDiagnosticReportLoading(true);

        try
        {
            DiagnosticReportResponse? report = await apiService.GenerateDiagnosticReportAsync();

            if (report != null)
            {
                Diagnostics.UpdateDiagnosticReport(report);
            }

            await LoadDiagnosticReportHistoryAsync(updateCurrentReport: false);

            Settings.UpdateRuntimeStatus();
        }
        finally
        {
            Diagnostics.SetDiagnosticReportLoading(false);
        }
    }

    private async Task RefreshDiagnosticReportsAsync()
    {
        Reports.SetDiagnosticReportHistoryLoading(true);

        try
        {
            await LoadDiagnosticReportHistoryAsync(updateCurrentReport: true);
            Settings.UpdateRuntimeStatus();
        }
        finally
        {
            Reports.SetDiagnosticReportHistoryLoading(false);
        }
    }

    private async Task LoadDiagnosticReportHistoryAsync(bool updateCurrentReport)
    {
        List<DiagnosticReportResponse> reports = await apiService.GetDiagnosticReportHistoryAsync();

        Reports.UpdateDiagnosticReportHistory(reports);

        if (!updateCurrentReport)
        {
            return;
        }

        DiagnosticReportResponse? latestReport = reports
            .OrderByDescending(report => report.CreatedAt)
            .FirstOrDefault();

        Diagnostics.UpdateDiagnosticReport(latestReport);
    }
}