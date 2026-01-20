namespace MyNotes.Common.Messages;

public interface IMessageToken<T> where T : notnull
{
  public string Key { get; }
  public T Context { get; }
}