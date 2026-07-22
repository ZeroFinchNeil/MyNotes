using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace MyNotes.Templates.Media;

public sealed partial class ExtendedAcrylicBackdrop : ExtendedSystemBackdrop
{
  DesktopAcrylicController? _desktopAcrylicController;

  protected override void OnBackdropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ExtendedAcrylicBackdrop backdrop && backdrop._desktopAcrylicController is DesktopAcrylicController controller)
    {
      controller.ResetProperties();
      controller.TintColor = backdrop.TintColor;
      controller.TintOpacity = (float)backdrop.TintOpacity;
      controller.LuminosityOpacity = (float)backdrop.LuminosityOpacity;
      controller.FallbackColor = backdrop.FallbackColor;
    }
  }

  protected override void OnDefaultSystemBackdropConfigurationChanged(ICompositionSupportsSystemBackdrop target, XamlRoot xamlRoot)
  {
  }

  protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
  {
    // Call the base method to initialize the default configuration object.
    base.OnTargetConnected(connectedTarget, xamlRoot);

    _desktopAcrylicController ??= new DesktopAcrylicController() { TintColor = TintColor, TintOpacity = (float)TintOpacity, LuminosityOpacity = (float)LuminosityOpacity, FallbackColor = FallbackColor };

    // Set configuration.
    SystemBackdropConfiguration defaultConfig = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
    SystemBackdropConfiguration customConfig = new()
    {
      Theme = defaultConfig.Theme,
      IsHighContrast = defaultConfig.IsHighContrast,
      HighContrastBackgroundColor = defaultConfig.HighContrastBackgroundColor,
      IsInputActive = true
    };
    _desktopAcrylicController.SetSystemBackdropConfiguration(customConfig);

    // Add target.
    _desktopAcrylicController.AddSystemBackdropTarget(connectedTarget);

  }

  protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
  {
    base.OnTargetDisconnected(disconnectedTarget);

    _desktopAcrylicController?.RemoveSystemBackdropTarget(disconnectedTarget);
    _desktopAcrylicController?.Dispose();
    _desktopAcrylicController = null;
  }
}

