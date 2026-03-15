using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace MyNotes.Templates.Media;

public sealed partial class ExtendedMicaBackdrop : ExtendedSystemBackdrop
{
  MicaController? _micaController;

  protected override void OnBackdropPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ExtendedMicaBackdrop backdrop && backdrop._micaController is MicaController controller)
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

    _micaController ??= new MicaController() { TintColor = TintColor, TintOpacity = (float)TintOpacity, LuminosityOpacity = (float)LuminosityOpacity, FallbackColor = FallbackColor };

    // Set configuration.
    SystemBackdropConfiguration defaultConfig = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
    _micaController.SetSystemBackdropConfiguration(defaultConfig);

    // Add target.
    _micaController.AddSystemBackdropTarget(connectedTarget);

  }

  protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
  {
    base.OnTargetDisconnected(disconnectedTarget);

    _micaController?.RemoveSystemBackdropTarget(disconnectedTarget);
    _micaController?.Dispose();
    _micaController = null;
  }
}
