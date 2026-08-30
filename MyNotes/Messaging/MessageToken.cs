namespace MyNotes.Messaging;

public readonly record struct MessageToken<T> : IMessageToken<T> where T : notnull 
{
  public static MessageToken<T> Create(T key) => new(key);

  private MessageToken(T key) => Key = key;

  public T Key { get; }
}
