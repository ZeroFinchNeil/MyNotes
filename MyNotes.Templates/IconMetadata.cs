using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Templates;

public record IconMetadata : IComparable<IconMetadata>
{
  public required short Id { get; set; }
  public required string Category { get; set; }
  public required string Group { get; set; }
  public required HashSet<string> Keywords { get; set; }
  public string? Description { get; set; }
  public string? Skintone { get; set; }
  public string? Unicode16 { get; set; }
  public int[]? Unicode32CodePoints { get; set; }

  public int CompareTo(IconMetadata? other) => other is null ? 1 : this.Id.CompareTo(other.Id);
}