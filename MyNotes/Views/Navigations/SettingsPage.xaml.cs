using Microsoft.Extensions.DependencyInjection;

using MyNotes.Shared.Constants;
using MyNotes.ViewModels;

using Windows.System;

namespace MyNotes.Views.Navigations;

[Debugging.ReferenceTracker]
internal sealed partial class SettingsPage : Page
{
  private readonly IServiceScope ServiceScope;
  private readonly SettingsViewModel ViewModel;
  private readonly DispatcherTimer _startupTaskTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };

  #region Object Lifetime Management
  public SettingsPage()
  {
    TrackReference();
    InitializeComponent();
    ServiceScope = App.Services.CreateScope();
    ViewModel = ServiceScope.ServiceProvider.GetRequiredService<SettingsViewModel>();

    _ = CheckStartupState();
    _startupTaskTimer.Tick += StartupTaskTimer_Tick;
    _startupTaskTimer.Start();

    // 초기 VisualState 결정
    if (ViewModel.IsAppLanguageChanged)
    {
      VisualStateManager.GoToState(this, nameof(SettingsPage_LanguageSettingsWarningState), false);
    }

    this.Loaded += SettingsPage_Loaded;
    this.Unloaded += SettingsPage_Unloaded;
  }

  private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
  {
    // 바인딩 해제
    Bindings.StopTracking();

    // StartupTaskTimer 정지 및 해제
    _startupTaskTimer.Stop();
    _startupTaskTimer.Tick -= StartupTaskTimer_Tick;

    ServiceScope.Dispose();
  }
  #endregion

  private bool _preventToggleChanging = false;
  private async Task CheckStartupState()
  {
    _preventToggleChanging = true;

    bool state = await ViewModel.GetStartupTaskState();
    SettingsPage_General_StartupToggleSwitch.IsOn = state;

    if (state)
    {
      VisualStateManager.GoToState(this, nameof(SettingsPage_StartupSettingsNormalState), false);
    }

    _preventToggleChanging = false;
  }
  private async void StartupTaskTimer_Tick(object? sender, object e) => await CheckStartupState();

  private async void SettingsPage_General_StartupToggleSwitch_Toggled(object sender, RoutedEventArgs e)
  {
    if (_preventToggleChanging)
    {
      return;
    }

    bool changedState = await ViewModel.ToggleStartupTaskState();

    if (SettingsPage_General_StartupToggleSwitch.IsOn == changedState)
    {
      VisualStateManager.GoToState(this, nameof(SettingsPage_StartupSettingsNormalState), false);
    }
    else
    {
      _preventToggleChanging = true;
      SettingsPage_General_StartupToggleSwitch.IsOn = changedState;
      if (!changedState)
      {
        VisualStateManager.GoToState(this, nameof(SettingsPage_StartupSettingsWarningState), false);
      }

      _preventToggleChanging = false;
    }
  }

  private void SettingsPage_Appearance_RestartButton_Click(object sender, RoutedEventArgs e)
  {
    AppInstance.Restart(string.Empty);
  }

  private async void SettingsPage_General_AppStartupButton_Click(object sender, RoutedEventArgs e)
  {
    await Launcher.LaunchUriAsync(new Uri($"ms-settings:appsfeatures-app?{Uri.EscapeDataString(AppStrings.PackageFamilyName)}"));
  }
}
