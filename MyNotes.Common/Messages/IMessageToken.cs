namespace MyNotes.Common.Messages;

internal interface IMessageToken<T> where T : notnull
{
  public string Key { get; }
  public T Context { get; }
}