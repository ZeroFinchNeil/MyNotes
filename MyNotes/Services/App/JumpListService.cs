using MyNotes.AppConstants;
using MyNotes.Models.Notes;
using MyNotes.Resources;

using Windows.UI.StartScreen;

namespace MyNotes.Services.App;

internal sealed class JumpListService
{
  public JumpListService()
  {
    _ = InitializeJumpListAsync();
  }

  private string GetArgument(Guid id) => @$"""JumpList_{id.ToString()}""";

  public async Task<bool> AddToJumpListAsync(Note note)
  {
    JumpList jumpList = await JumpList.LoadCurrentAsync();
    string argument = GetArgument(note.Id.Value);
    if (jumpList.Items.FirstOrDefault(item => item.Arguments == argument) is not null)
    {
      return false;
    }
    var newItem = JumpListItem.CreateWithArguments(argument, note.Title);
    newItem.Logo = new Uri("ms-appx:///Assets/AppIcon.ico");
    jumpList.Items.Add(newItem);
    await jumpList.SaveAsync();
    return true;
  }

  public async Task<bool> EditJumpListItemAsync(Note note)
  {
    JumpList jumpList = await JumpList.LoadCurrentAsync();
    string argument = GetArgument(note.Id.Value);
    if (jumpList.Items.FirstOrDefault(item => item.Arguments == argument) is JumpListItem jumpListItem)
    {
      jumpListItem.DisplayName = note.Title;
      await jumpList.SaveAsync();
      return true;
    }
    return false;
  }

  public async Task<bool> RemoveFromJumpListAsync(NoteId id)
  {
    JumpList jumpList = await JumpList.LoadCurrentAsync();
    string argument = GetArgument(id.Value);
    if (jumpList.Items.FirstOrDefault(item => item.Arguments == argument) is JumpListItem item)
    {
      return jumpList.Items.Remove(item);
    }
    return false;
  }

  public async Task InitializeJumpListAsync()
  {
    JumpList jumpList = await JumpList.LoadCurrentAsync();
    jumpList.Items.Clear();
    jumpList.SystemGroupKind = JumpListSystemGroupKind.None;
    var newNoteItem = JumpListItem.CreateWithArguments(AppStrings.LaunchArgument_JumpList_NewNote, LocalizedStrings.JumpListNewNote);
    newNoteItem.Logo = new Uri("ms-appx:///Assets/Icons/Main/3145");
    var viewListItem = JumpListItem.CreateWithArguments(AppStrings.LaunchArgument_JumpList_MainWindow, LocalizedStrings.JumpListMainWindow);
    viewListItem.Logo = new Uri("ms-appx:///Assets/Icons/Main/3155");
    var settingsItem = JumpListItem.CreateWithArguments(AppStrings.LaunchArgument_JumpList_Settings, LocalizedStrings.JumpListSettings);
    settingsItem.Logo = new Uri("ms-appx:///Assets/Icons/Main/3316");
    jumpList.Items.Add(newNoteItem);
    jumpList.Items.Add(viewListItem);
    jumpList.Items.Add(settingsItem);
    await jumpList.SaveAsync();
  }
}
