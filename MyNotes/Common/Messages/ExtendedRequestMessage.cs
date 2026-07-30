using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MyNotes.Common.Messages;

public class ExtendedRequestMessage<TRequest, UResponse> : RequestMessage<UResponse>
{
  public required TRequest Request { get; init; }
}
