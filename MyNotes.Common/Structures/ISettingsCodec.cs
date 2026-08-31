namespace MyNotes.Common.Structures;

public interface ISettingsCodec<T, TSettingsSupported>
{
  public TSettingsSupported Encode(T settings);
  public T Decode(TSettingsSupported settings);
}