using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MyNotes.Messaging;

public class ExtendedRequestMessage<TRequest, UResponse> : RequestMessage<UResponse>
{
  public required TRequest Request { get; init; }
}
