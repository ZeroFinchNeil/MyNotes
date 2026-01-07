namespace MyNotes.Models.UI;

internal readonly struct WindowPresenterState
{
  public required WindowActivationState WindowActivationState { get; init; }
  public required OverlappedPresenterState OverlappedPresenterState { get; init; }
}
