using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Common.Structures;

namespace MyNotes.Application.Navigations;

internal static class NavigationSettingsDescriptors
{
  public static SettingsDescriptor<NoteSortKey> NoteSortKey { get; } = new()
  {
    Key = "NoteSortKey",
    DefaultValue = Contracts.Notes.Models.NoteSortKey.Created
  };

  public static SettingsDescriptor<SortDirection> NoteSortDirection { get; } = new()
  {
    Key = "NoteSortDirection",
    DefaultValue = SortDirection.Descending
  };

  public static SettingsDescriptor<PreviewLayoutType> PreviewLayoutType { get; } = new()
  {
    Key = "PreviewLayoutType",
    DefaultValue = Contracts.Navigations.Models.PreviewLayoutType.Grid
  };

  public static SettingsDescriptor<PreviewTileSize> PreviewTileSize { get; } = new()
  {
    Key = "PreviewTileSize",
    DefaultValue = Contracts.Navigations.Models.PreviewTileSize.Medium
  };

  public static SettingsDescriptor<PreviewTileRatio> PreviewTileRatio { get; } = new()
  {
    Key = "PreviewTileRatio",
    DefaultValue = Contracts.Navigations.Models.PreviewTileRatio.Square
  };
}