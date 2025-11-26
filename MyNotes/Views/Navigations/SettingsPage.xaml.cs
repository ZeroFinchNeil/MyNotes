using Microsoft.Extensions.DependencyInjection;

using MyNotes.ViewModels;

namespace MyNotes.Views.Navigations;

public sealed partial class SettingsPage : Page
{
  private readonly SettingsViewModel ViewModel;

  public SettingsPage()
  {
    InitializeComponent();
    ViewModel = App.Instance.Services.GetRequiredService<SettingsViewModel>();

    // 초기 VisualState 결정
    if (ViewModel.IsAppLanguageChanged)
      VisualStateManager.GoToState(this, "SettingsPage_LanguageSettingsWarningState", false);

    this.Unloaded += SettingsPage_Unloaded;
  }

  private void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
  {
    // 바인딩 해제
    Bindings.StopTracking();
  }

  private void SettingsPage_Appearence_RelaunchButton_Click(object sender, RoutedEventArgs e)
  {
    AppInstance.Restart(string.Empty);
  }
}
