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

  public static SettingsDescriptor<NoteSortKey> BookmarksPageNoteSortKey { get; } = new()
  {
    Key = "BookmarksPageNoteSortKey",
    DefaultValue = Contracts.Notes.Models.NoteSortKey.Created
  };

  public static SettingsDescriptor<SortDirection> BookmarksPageNoteSortDirection { get; } = new()
  {
    Key = "BookmarksPageNoteSortDirection",
    DefaultValue = SortDirection.Descending
  };

  public static SettingsDescriptor<PreviewLayoutType> BookmarksPagePreviewLayoutType { get; } = new()
  {
    Key = "BookmarksPagePreviewLayoutType",
    DefaultValue = Contracts.Navigations.Models.PreviewLayoutType.Grid
  };

  public static SettingsDescriptor<PreviewTileSize> BookmarksPagePreviewTileSize { get; } = new()
  {
    Key = "BookmarksPagePreviewTileSize",
    DefaultValue = Contracts.Navigations.Models.PreviewTileSize.Medium
  };

  public static SettingsDescriptor<PreviewTileRatio> BookmarksPagePreviewTileRatio { get; } = new()
  {
    Key = "BookmarksPagePreviewTileRatio",
    DefaultValue = Contracts.Navigations.Models.PreviewTileRatio.Square
  };

  public static SettingsDescriptor<NoteSortKey> TrashPageNoteSortKey { get; } = new()
  {
    Key = "TrashPageNoteSortKey",
    DefaultValue = Contracts.Notes.Models.NoteSortKey.Created
  };

  public static SettingsDescriptor<SortDirection> TrashPageNoteSortDirection { get; } = new()
  {
    Key = "TrashPageNoteSortDirection",
    DefaultValue = SortDirection.Descending
  };

  public static SettingsDescriptor<PreviewLayoutType> TrashPagePreviewLayoutType { get; } = new()
  {
    Key = "TrashPagePreviewLayoutType",
    DefaultValue = Contracts.Navigations.Models.PreviewLayoutType.Grid
  };

  public static SettingsDescriptor<PreviewTileSize> TrashPagePreviewTileSize { get; } = new()
  {
    Key = "TrashPagePreviewTileSize",
    DefaultValue = Contracts.Navigations.Models.PreviewTileSize.Medium
  };

  public static SettingsDescriptor<PreviewTileRatio> TrashPagePreviewTileRatio { get; } = new()
  {
    Key = "TrashPagePreviewTileRatio",
    DefaultValue = Contracts.Navigations.Models.PreviewTileRatio.Square
  };
}