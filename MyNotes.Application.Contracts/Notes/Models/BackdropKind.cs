using MyNotes.Common.Structures;

namespace MyNotes.Application.Contracts.Notes.Models;

public enum BackdropKind
{
  None,
  Acrylic,
  Mica
}

public sealed class BackdropKindSettingsCodec : ISettingsCodec<BackdropKind, int>
{
  public static BackdropKindSettingsCodec Default => field ??= new();
  private BackdropKindSettingsCodec() { }

  public int Encode(BackdropKind input) => (int)input;

  public BackdropKind Decode(int output) => (BackdropKind)output;
}