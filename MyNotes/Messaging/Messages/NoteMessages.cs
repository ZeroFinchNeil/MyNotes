using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Domain.Notes;
using MyNotes.Models.UI;

namespace MyNotes.Messaging.Messages;

internal sealed class NoteTitleChangedMessage(object sender, string? propertyName, string oldValue, string newValue) : PropertyChangedMessage<string>(sender, propertyName, oldValue, newValue)
{
}

internal sealed class NoteBookmarkedChangedMessage(object sender, string? propertyName, bool oldValue, bool newValue) : PropertyChangedMessage<bool>(sender, propertyName, oldValue, newValue)
{
}

internal sealed record NoteAdditionRequestedMessage(NoteId NoteId)
{
}

internal sealed record NotePreviewUpdateRequestedMessage(NoteId NoteId)
{
}

internal sealed class NoteWindowActivationChangedMessage(WindowPresenterState value) : ValueChangedMessage<WindowPresenterState>(value)
{
}