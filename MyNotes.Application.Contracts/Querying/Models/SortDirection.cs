using MyNotes.Common.Structures;

namespace MyNotes.Application.Contracts.Querying.Models;

public enum SortDirection
{
  Ascending,
  Descending
}

public sealed class SortDirectionSettingsCodec : ISettingsCodec<SortDirection, int>
{
  public static SortDirectionSettingsCodec Default => field ??= new();

  private SortDirectionSettingsCodec() { }
  public int Encode(SortDirection input) => (int)input;

  public SortDirection Decode(int output) => (SortDirection)output;
}