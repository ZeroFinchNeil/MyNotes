using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyNotes.Services.Database.Entities;

internal sealed class NavigationEntity : IEquatable<NavigationEntity>
{
  [Key]
  public required Guid Id { get; init; }

  public required string Title { get; set; }

  public required short Icon { get; set; }

  public required Guid Parent { get; set; }

  public required int Position { get; set; }

  public required bool IsComposite { get; init; }

  public required bool IsExpanded { get; set; }

  public required bool IsDeleted { get; set; }

  public Guid? RestorePrevious { get; set; }

  public Guid? RestoreNext { get; set; }

  private static string ToStringValue(string name, object? value) => string.Format("{0,12} | {1}", name, value?.ToString() ?? "Null");
  public override string ToString()
  {
    StringBuilder sb = new();
    sb.AppendLine(ToStringValue(nameof(Id), Id));
    sb.AppendLine(ToStringValue(nameof(Title), Title));
    sb.AppendLine(ToStringValue(nameof(Icon), Icon));
    sb.AppendLine(ToStringValue(nameof(Parent), Parent));
    sb.AppendLine(ToStringValue(nameof(Position), Position));
    sb.AppendLine(ToStringValue(nameof(IsComposite), IsComposite));
    sb.AppendLine(ToStringValue(nameof(IsExpanded), IsExpanded));
    sb.AppendLine(ToStringValue(nameof(IsDeleted), IsDeleted));
    sb.AppendLine(ToStringValue(nameof(RestorePrevious), RestorePrevious));
    sb.AppendLine(ToStringValue(nameof(RestoreNext), RestoreNext));
    return sb.ToString();
  }

  public bool Equals(NavigationEntity? other) => other is not null && other.Id == Id;

  public override bool Equals(object? obj) => this.Equals(obj as NavigationEntity);
  public override int GetHashCode() => Id.GetHashCode();
}
