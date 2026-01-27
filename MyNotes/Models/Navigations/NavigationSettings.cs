using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle
  };

  private NavigationSettings() : base(typeof(SettingsPage)) { }
}
