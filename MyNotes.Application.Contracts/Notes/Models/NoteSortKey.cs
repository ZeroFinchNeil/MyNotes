using MyNotes.Common.Structures;

namespace MyNotes.Application.Contracts.Notes.Models;

public enum NoteSortKey
{
  Modified,
  Created,
  Title
}

public sealed class NoteSortKeySettingsCodec : ISettingsCodec<NoteSortKey, int>
{
  public static NoteSortKeySettingsCodec Default => field ??= new();

  private NoteSortKeySettingsCodec() { }

  public int Encode(NoteSortKey input) => (int)input;

  public NoteSortKey Decode(int output) => (NoteSortKey)output;
}