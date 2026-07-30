namespace MyNotes.Application.Contracts.Notes.Models;

public enum BackdropKind
{
  None,
  Acrylic,
  Mica
}

public static class BackdropKindSettingsCodec
{
  public static int Encode(BackdropKind input) => (int)input;

  public static BackdropKind Decode(int output) => (BackdropKind)output;
}