using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Domain.Navigations;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations.Core;

internal sealed partial class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Settings,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle
  };

  private NavigationSettings() : base(typeof(SettingsPage)) { }
}
