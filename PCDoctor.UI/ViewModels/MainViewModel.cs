using System.Windows.Input;
using PCDoctor.Core.Models;
using PCDoctor.Core.Services;
using PCDoctor.UI.Commands;
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

    public SettingsViewModel Settings { get; }

    public ICommand OpenSettingsCommand { get; }
    public ICommand GenerateDiagnosticReportCommand { get; }

    public MainViewModel(
        AppSettings settings,
        SettingsService settingsService,
        MonitoringService monitoringService,
        ApiService apiService,
        DashboardFormatter formatter,
        WindowService windowService)
    {
        this.settings = settings;
        this.monitoringService = monitoringService;
        this.apiService = apiService;

        Dashboard = new DashboardViewModel(formatter);
        Settings = new SettingsViewModel(settings, settingsService, apiService);

        OpenSettingsCommand = new RelayCommand(windowService.OpenSettingsWindow);
        GenerateDiagnosticReportCommand = new RelayCommand(() => _ = GenerateDiagnosticReportAsync());
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
            OnPropertyChanged(nameof(IsHistorySelected));
            OnPropertyChanged(nameof(IsProcessesSelected));
            OnPropertyChanged(nameof(IsSettingsSelected));
        }
    }

    public string PageTitle => SelectedPage switch
    {
        "Overview" => "Overview",
        "Diagnostics" => "Diagnostics",
        "History" => "History",
        "Processes" => "Processes",
        "Settings" => "Settings",
        _ => "PCDoctor"
    };

    public string PageSubtitle => SelectedPage switch
    {
        "Overview" => "Important system information at a glance.",
        "Diagnostics" => "Detailed diagnostic reports and recommendations.",
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
            while (!token.IsCancellationRequested)
            {
                var result = await monitoringService.GetMonitoringResultAsync();

                Dashboard.Update(result);
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
        Dashboard.SetDiagnosticReportLoading(true);

        try
        {
            var report = await apiService.GenerateDiagnosticReportAsync();
            Dashboard.UpdateDiagnosticReport(report);
            Settings.UpdateRuntimeStatus();
        }
        finally
        {
            Dashboard.SetDiagnosticReportLoading(false);
        }
    }
}