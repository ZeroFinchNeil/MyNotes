using MyNotes.Common.Structures;

namespace MyNotes.Application.Contracts.Navigations.Models;

public enum PreviewLayoutType
{
  Grid,
  List
}

internal sealed class PreviewLayoutTypeSettingsCodec : ISettingsCodec<PreviewLayoutType, int>
{
  public static PreviewLayoutTypeSettingsCodec Default => field ??= new();

  private PreviewLayoutTypeSettingsCodec() { }

  public int Encode(PreviewLayoutType input) => (int)input;

  public PreviewLayoutType Decode(int output) => (PreviewLayoutType)output;
}