using Microsoft.Extensions.DependencyInjection;

namespace MyNotes.ViewModels.Dialogs;

internal sealed class DialogViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
  private readonly IServiceProvider ServiceProvider = serviceProvider;

  public DialogViewModelBase Resolve(DialogType dialogType, params object[] parameters) => dialogType switch
  {
    DialogType.SetNode => ActivatorUtilities.CreateInstance<EditUserNavigationDialogViewModel>(ServiceProvider, parameters),
    DialogType.ConfirmDelete => ActivatorUtilities.CreateInstance<ConfirmDeleteDialogViewModel>(ServiceProvider, parameters),
    _ => throw new ArgumentException("Invalid DialogType")
  };
}
