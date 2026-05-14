namespace MyNotes.Models.UI;

internal sealed record DialogResponse<T>
{
  public required ContentDialogResult Result { get; init; }

  public required T Data { get; init; }
}