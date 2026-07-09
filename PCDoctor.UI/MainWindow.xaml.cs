using System.Windows;
using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.UI.Services;
using PCDoctor.UI.ViewModels;

namespace PCDoctor.UI;

public partial class MainWindow : Window
{
    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;
    private readonly SettingsService settingsService;
    private readonly WindowService windowService;
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();

        settingsService = new SettingsService();
        settings = settingsService.LoadSettings();

        monitor = new SystemMonitor();
        apiService = new ApiService(settings);
        windowService = new WindowService(settings, settingsService, apiService);

        DashboardFormatter formatter = new();

        MonitoringService monitoringService = new(
            settings,
            monitor,
            apiService
        );

        viewModel = new MainViewModel(
            settings,
            settingsService,
            monitoringService,
            apiService,
            formatter,
            windowService
        );

        DataContext = viewModel;

        viewModel.StartMonitoring();
    }

    protected override void OnClosed(EventArgs e)
    {
        viewModel.StopMonitoring();
        base.OnClosed(e);
    }
}