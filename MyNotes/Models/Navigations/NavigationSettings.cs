using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Domain.ValueObjects;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Settings,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle
  };

  private NavigationSettings() : base(typeof(SettingsPage)) { }
}
