namespace MyNotes.Common.Messages;

public readonly record struct MessageToken : IMessageToken<MessageTokenContextNull>
{
  public required string Key { get; init; }
  public MessageTokenContextNull Context { get; } = MessageTokenContextNull.Value;

  public MessageToken() { }
}

public readonly record struct MessageToken<T> : IMessageToken<T> where T : notnull
{
  public required string Key { get; init; }
  public required T Context { get; init; }
}

public sealed class MessageTokenContextNull
{
  private MessageTokenContextNull() { }
  public static MessageTokenContextNull Value { get; } = new();
}
