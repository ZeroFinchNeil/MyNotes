namespace MyNotes.Application.Contracts.Querying.Models;

public enum SortDirection
{
  Ascending,
  Descending
}

public static class SortDirectionSettingsCodec
{
  public static int Encode(SortDirection input) => (int)input;

  public static SortDirection Decode(int output) => (SortDirection)output;
}