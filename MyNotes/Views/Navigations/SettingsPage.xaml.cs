using Microsoft.Extensions.DependencyInjection;

using MyNotes.ViewModels;

using Windows.System;

namespace MyNotes.Views.Navigations;

public sealed partial class SettingsPage : Page
{
  private readonly SettingsViewModel ViewModel;
  private readonly DispatcherTimer _startupTaskTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };

  public SettingsPage()
  {
    InitializeComponent();
    ViewModel = App.Instance.Services.GetRequiredService<SettingsViewModel>();

    _ = CheckStartupState();
    _startupTaskTimer.Tick += StartupTaskTimer_Tick;
    _startupTaskTimer.Start();

    // 초기 VisualState 결정
    if (ViewModel.IsAppLanguageChanged)
      VisualStateManager.GoToState(this, "SettingsPage_LanguageSettingsWarningState", false);

    this.Unloaded += SettingsPage_Unloaded;
  }

  private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
  {
    // 바인딩 해제
    Bindings.StopTracking();

    // StartupTaskTimer 정지 및 해제
    _startupTaskTimer.Start();
    _startupTaskTimer.Tick -= StartupTaskTimer_Tick;
  }

  private bool _preventToggleChanging = false;
  private async Task CheckStartupState()
  {
    Console.WriteLine("Tick");
    _preventToggleChanging = true;

    bool state = await ViewModel.GetStartupTaskState();
    SettingsPage_General_StartupToggleSwitch.IsOn = state;

    if (state)
      VisualStateManager.GoToState(this, "SettingsPage_StartupSettingsNormalState", false);

    _preventToggleChanging = false;
  }
  private async void StartupTaskTimer_Tick(object? sender, object e) => await CheckStartupState();

  private async void SettingsPage_General_StartupToggleSwitch_Toggled(object sender, RoutedEventArgs e)
  {
    if (_preventToggleChanging)
      return;

    bool changedState = await ViewModel.ToggleStartupTaskState();

    if (SettingsPage_General_StartupToggleSwitch.IsOn == changedState)
      VisualStateManager.GoToState(this, "SettingsPage_StartupSettingsNormalState", false);
    else
    {
      _preventToggleChanging = true;
      SettingsPage_General_StartupToggleSwitch.IsOn = changedState;
      if (!changedState)
        VisualStateManager.GoToState(this, "SettingsPage_StartupSettingsWarningState", false);
      _preventToggleChanging = false;
    }
  }

  private void SettingsPage_Appearance_RelaunchButton_Click(object sender, RoutedEventArgs e)
  {
    AppInstance.Restart(string.Empty);
  }

  private async void SettingsPage_General_AppStartupButton_Click(object sender, RoutedEventArgs e)
  {
    await Launcher.LaunchUriAsync(new Uri($"ms-settings:appsfeatures-app?{Uri.EscapeDataString(App.PackageFamilyName)}"));
  }
}
