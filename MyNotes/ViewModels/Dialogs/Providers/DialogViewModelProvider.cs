using Microsoft.Extensions.DependencyInjection;

namespace MyNotes.ViewModels.Dialogs.Providers;

internal sealed class DialogViewModelProvider(IServiceProvider serviceProvider) : IViewModelProvider<DialogType, ReadOnlySpan<object>, DialogViewModelBase>
{
  public IViewModelLease<DialogViewModelBase> Resolve(DialogType dialogType, params ReadOnlySpan<object> parameters)
  {
    DialogViewModelBase viewmodel = dialogType switch
    {
      DialogType.EditUserNavigation => ActivatorUtilities.CreateInstance<EditUserNavigationDialogViewModel>(serviceProvider, [.. parameters]),
      DialogType.ConfirmDelete => ActivatorUtilities.CreateInstance<ConfirmDeleteDialogViewModel>(serviceProvider, [.. parameters]),
      DialogType.SelectNoteParent => ActivatorUtilities.CreateInstance<SelectNoteParentDialogViewModel>(serviceProvider, [.. parameters]),
      _ => throw new ArgumentException("Invalid DialogType")
    };

    return new ViewModelLease() { ViewModel = viewmodel };
  }

  IViewModelLease<DialogViewModelBase>? IViewModelProvider<DialogType, ReadOnlySpan<object>, DialogViewModelBase>.Acquire(DialogType dialogType) => throw new NotImplementedException();

  private sealed class ViewModelLease() : IViewModelLease<DialogViewModelBase>
  {
    public required DialogViewModelBase ViewModel { get; init; }

    public bool Disposed { get; private set; }

    private void Dispose(bool disposing)
    {
      if (Disposed)
      {
        return;
      }

      if (disposing)
      {
        ViewModel.Dispose();
      }

      Disposed = true;
    }

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
