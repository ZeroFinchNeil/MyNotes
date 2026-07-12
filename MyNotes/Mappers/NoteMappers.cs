using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Models.Notes;
using MyNotes.Shared.Enums.Notes;

using ColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Mappers;

internal static class NoteMappers
{
  public static NoteModel ToModel(NoteBundleAppResponseDto bundleDto)
  {
    var noteDto = bundleDto.NoteDto;
    var viewStateDto = bundleDto.ViewStateDto;

    return new()
    {
      Id = noteDto.Id,
      NavigationId = noteDto.NavigationId,
      Created = noteDto.Created,
      Title = noteDto.Title,
      Body = noteDto.Body,
      BodyPlainText = noteDto.BodyPlainText,
      IsBookmarked = noteDto.IsBookmarked,
      IsDeleted = noteDto.IsDeleted,
      BackgroundColor = ColorHelper.ToColor(noteDto.BackgroundColor),
      ShowBackgroundImage = viewStateDto.ShowBackgroundImage,
      BackgroundImagePath = viewStateDto.BackgroundImagePath,
      BackgroundImageOpacity = viewStateDto.BackgroundImageOpacity,
      BackgroundImageBlur = viewStateDto.BackgroundImageBlur,
      BackdropKind = (BackdropKind)viewStateDto.BackdropKind,
      BackdropTintOpacity = viewStateDto.BackdropTintOpacity,
      BackdropLuminosityOpacity = viewStateDto.BackdropLuminosityOpacity,
      Images = [],
      ShowImagePanel = viewStateDto.ShowImagePanel,
      ImagePanelHeight = viewStateDto.ImagePanelHeight,
      Size = new(viewStateDto.Width, viewStateDto.Height),
      Position = new(viewStateDto.PositionX, viewStateDto.PositionY),
      IsWindowOpen = viewStateDto.IsWindowOpen,
      IsAlwaysOnTop = viewStateDto.IsAlwaysOnTop
    };
  }

  public static Note ToDomain(NoteModel noteModel) => throw new NotImplementedException();

  public static void Apply(NoteModel noteModel, NoteBundleAppResponseDto bundleDto)
  {
    var noteDto = bundleDto.NoteDto;
    var viewStateDto = bundleDto.ViewStateDto;

    if (noteModel.Id != noteDto.Id)
    {
      throw new ArgumentException("ID가 일치하지 않습니다.", nameof(noteModel));
    }

    noteModel.NavigationId = noteDto.NavigationId;
    noteModel.Title = noteDto.Title;
    noteModel.Body = noteDto.Body;
    noteModel.BodyPlainText = noteDto.BodyPlainText;
    noteModel.IsBookmarked = noteDto.IsBookmarked;
    noteModel.IsDeleted = noteDto.IsDeleted;
    noteModel.BackgroundColor = ColorHelper.ToColor(noteDto.BackgroundColor);
    noteModel.ShowBackgroundImage = viewStateDto.ShowBackgroundImage;
    noteModel.BackgroundImagePath = viewStateDto.BackgroundImagePath;
    noteModel.BackgroundImageOpacity = viewStateDto.BackgroundImageOpacity;
    noteModel.BackgroundImageBlur = viewStateDto.BackgroundImageBlur;
    noteModel.BackdropKind = (BackdropKind)viewStateDto.BackdropKind;
    noteModel.BackdropTintOpacity = viewStateDto.BackdropTintOpacity;
    noteModel.BackdropLuminosityOpacity = viewStateDto.BackdropLuminosityOpacity;
    noteModel.Images = [];
    noteModel.ShowImagePanel = viewStateDto.ShowImagePanel;
    noteModel.ImagePanelHeight = viewStateDto.ImagePanelHeight;
    noteModel.Size = new(viewStateDto.Width, viewStateDto.Height);
    noteModel.Position = new(viewStateDto.PositionX, viewStateDto.PositionY);
    noteModel.IsWindowOpen = viewStateDto.IsWindowOpen;
    noteModel.IsAlwaysOnTop = viewStateDto.IsAlwaysOnTop;
  }
}

internal static class NoteMappingExtensions
{
  extension(NoteModel model)
  {
    public void Apply(NoteBundleAppResponseDto noteDto) => NoteMappers.Apply(model, noteDto);
  }

  extension(NoteBundleAppResponseDto dto)
  {
    public NoteModel ToModel() => NoteMappers.ToModel(dto);
  }
}