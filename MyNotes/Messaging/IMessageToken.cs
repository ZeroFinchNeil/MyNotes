namespace MyNotes.Messaging;

public interface IMessageToken<T> where T : notnull
{
  public T Key { get; }
}