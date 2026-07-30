namespace MyNotes.Application.Contracts.Notes.Models;

public enum NoteSortKey
{
  Modified,
  Created,
  Title
}

public static class NoteSortKeySettingsCodec
{
  public static int Encode(NoteSortKey input) => (int)input;

  public static NoteSortKey Decode(int output) => (NoteSortKey)output;
}