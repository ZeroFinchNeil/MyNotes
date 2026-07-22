namespace MyNotes.Shell.Contracts.Converters;

public interface IRtfTextConverter
{
  public string ToPlainText(string? rtfText);

  public string GetPreview(string? body, int start, int end);
}
