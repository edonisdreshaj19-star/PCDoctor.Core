using System.Windows;
using System.Windows.Controls;
using PCDoctor.UI.ViewModels;

namespace PCDoctor.UI.Views.Pages;

public partial class SettingsView : UserControl
{
    private SettingsViewModel? subscribedViewModel;

    public SettingsView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SubscribeToSettingsViewModel();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnsubscribeFromSettingsViewModel();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UnsubscribeFromSettingsViewModel();
        SubscribeToSettingsViewModel();
    }

    private void SubscribeToSettingsViewModel()
    {
        if (DataContext is not MainViewModel mainViewModel)
        {
            return;
        }

        SettingsViewModel settingsViewModel = mainViewModel.Settings;

        if (subscribedViewModel == settingsViewModel)
        {
            return;
        }

        UnsubscribeFromSettingsViewModel();

        subscribedViewModel = settingsViewModel;
        subscribedViewModel.ConfirmationRequested += ShowConfirmation;
        subscribedViewModel.NotificationRequested += ShowNotification;
    }

    private void UnsubscribeFromSettingsViewModel()
    {
        if (subscribedViewModel == null)
        {
            return;
        }

        subscribedViewModel.ConfirmationRequested -= ShowConfirmation;
        subscribedViewModel.NotificationRequested -= ShowNotification;
        subscribedViewModel = null;
    }

    private bool ShowConfirmation(string message, string title)
    {
        Window? owner = Window.GetWindow(this);

        MessageBoxResult result = MessageBox.Show(
            owner,
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    private void ShowNotification(string message, string title)
    {
        Window? owner = Window.GetWindow(this);

        MessageBox.Show(
            owner,
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}