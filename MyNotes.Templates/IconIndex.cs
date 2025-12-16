using System.Collections.Generic;

namespace MyNotes.Templates;

public record IconIndex
{
  public required Dictionary<string, List<short>> Terms { get; set; }
}
