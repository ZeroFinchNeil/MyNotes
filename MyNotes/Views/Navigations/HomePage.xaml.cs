using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Views.Navigations;

internal sealed partial class HomePage : Page
{
  public HomePage()
  {
    InitializeComponent();
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    Note note = new() { Created = DateTime.Now, Id = NoteId.NewId(), NavigationId = NavigationId.NewId() };
    note.Title = $"New note {note.Id.Value}";
    NoteWindow noteWindow = new(note);
    noteWindow.Activate();
  }
}
