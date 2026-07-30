using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MyNotes.Common.Messages;

internal class EmptyMessage : ValueChangedMessage<object?>
{
  public EmptyMessage() : base(null) { }

  public string? Description { get; init; }
}
