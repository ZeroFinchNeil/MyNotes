using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;

using ColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Common.Mappers;

internal static class NoteMappers
{
  public static NoteModel ToModel(NoteDto noteDto)
  {
    var viewStateDto = noteDto.ViewStateDto;

    return new()
    {
      Id = noteDto.Id,
      NavigationId = noteDto.NavigationId,
      Created = noteDto.Created,
      Modified = noteDto.Modified,
      Title = noteDto.Title,
      Body = noteDto.Body,
      IsBookmarked = noteDto.IsBookmarked,
      IsDeleted = noteDto.IsDeleted,
      BackgroundColor = ColorHelper.ToColor(noteDto.BackgroundColor),
      ShowBackgroundImage = viewStateDto.ShowBackgroundImage,
      BackgroundImageStretch = (Stretch)viewStateDto.BackgroundImageStretch,
      BackgroundImageAlignment = (AlignmentPosition)viewStateDto.BackgroundImageAlignment,
      BackgroundImagePath = noteDto.BackgroundImagePath,
      BackgroundImageOpacity = viewStateDto.BackgroundImageOpacity,
      BackgroundImageBlur = viewStateDto.BackgroundImageBlur,
      BackdropKind = (BackdropKind)viewStateDto.BackdropKind,
      BackdropTintOpacity = viewStateDto.BackdropTintOpacity,
      BackdropLuminosityOpacity = viewStateDto.BackdropLuminosityOpacity,
      ShowImagePanel = viewStateDto.ShowImagePanel,
      ImagePanelHeight = viewStateDto.ImagePanelHeight,
      Size = new(viewStateDto.Width, viewStateDto.Height),
      Position = new(viewStateDto.PositionX, viewStateDto.PositionY),
      IsWindowOpen = viewStateDto.IsWindowOpen,
      IsAlwaysOnTop = viewStateDto.IsAlwaysOnTop,
      IsTextEditorReadOnly = viewStateDto.IsTextEditorReadOnly,
    };
  }

  public static Note ToDomain(NoteModel noteModel) => new()
  {
    Id = noteModel.Id,
    NavigationId = noteModel.NavigationId,
    Created = noteModel.Created,
    Modified = noteModel.Modified,
    Title = noteModel.Title,
    Body = noteModel.Body,
    BackgroundColor = noteModel.BackgroundColor.ToString(),
    BackgroundImagePath = noteModel.BackgroundImagePath,
    IsBookmarked = noteModel.IsBookmarked,
    IsDeleted = noteModel.IsDeleted
  };

  public static void Apply(NoteModel noteModel, NoteDto noteDto)
  {
    var viewStateDto = noteDto.ViewStateDto;

    if (noteModel.Id != noteDto.Id)
    {
      throw new ArgumentException("ID가 일치하지 않습니다.", nameof(noteModel));
    }

    noteModel.NavigationId = noteDto.NavigationId;
    noteModel.Title = noteDto.Title;
    noteModel.Body = noteDto.Body;
    noteModel.IsBookmarked = noteDto.IsBookmarked;
    noteModel.IsDeleted = noteDto.IsDeleted;
    noteModel.BackgroundColor = ColorHelper.ToColor(noteDto.BackgroundColor);
    noteModel.ShowBackgroundImage = viewStateDto.ShowBackgroundImage;
    noteModel.BackgroundImageStretch = (Stretch)viewStateDto.BackgroundImageStretch;
    noteModel.BackgroundImageAlignment = (AlignmentPosition)viewStateDto.BackgroundImageAlignment;
    noteModel.BackgroundImagePath = noteDto.BackgroundImagePath;
    noteModel.BackgroundImageOpacity = viewStateDto.BackgroundImageOpacity;
    noteModel.BackgroundImageBlur = viewStateDto.BackgroundImageBlur;
    noteModel.BackdropKind = (BackdropKind)viewStateDto.BackdropKind;
    noteModel.BackdropTintOpacity = viewStateDto.BackdropTintOpacity;
    noteModel.BackdropLuminosityOpacity = viewStateDto.BackdropLuminosityOpacity;
    noteModel.ShowImagePanel = viewStateDto.ShowImagePanel;
    noteModel.ImagePanelHeight = viewStateDto.ImagePanelHeight;
    noteModel.Size = new(viewStateDto.Width, viewStateDto.Height);
    noteModel.Position = new(viewStateDto.PositionX, viewStateDto.PositionY);
    noteModel.IsWindowOpen = viewStateDto.IsWindowOpen;
    noteModel.IsAlwaysOnTop = viewStateDto.IsAlwaysOnTop;
  }
}