using System.Windows;
using PCDoctor.Core.Models;
using PCDoctor.Core.Monitoring;
using PCDoctor.Core.Services;
using PCDoctor.UI.ViewModels;

namespace PCDoctor.UI;

public partial class MainWindow : Window
{
    private readonly AppSettings settings;
    private readonly SystemMonitor monitor;
    private readonly ApiService apiService;
    private readonly SettingsService settingsService;
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        
        settingsService = new SettingsService();
        settings = settingsService.LoadSettings();

        monitor = new SystemMonitor();
        apiService = new ApiService(settings);

        viewModel = new MainViewModel(settings, monitor, apiService);
        DataContext = viewModel;
        
        viewModel.StartMonitoring();
    }
    
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsWindow = new(settings, settingsService)
        {
            Owner = this
        };

        settingsWindow.ShowDialog();
    }
}