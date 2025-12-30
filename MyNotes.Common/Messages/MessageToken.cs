namespace MyNotes.Common.Messages;

internal readonly record struct MessageToken : IMessageToken<MessageTokenContextNull>
{
  public required string Key { get; init; }
  public MessageTokenContextNull Context { get; } = MessageTokenContextNull.Value;

  public MessageToken() { }
}

internal readonly record struct MessageToken<T> : IMessageToken<T> where T : notnull
{
  public required string Key { get; init; }
  public required T Context { get; init; }
}

internal sealed class MessageTokenContextNull
{
  private MessageTokenContextNull() { }
  public static MessageTokenContextNull Value { get; } = new();
}
