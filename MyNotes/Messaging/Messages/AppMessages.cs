using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MyNotes.Messaging.Messages;

internal sealed class AppThemeChangedMessage(ElementTheme value) : ValueChangedMessage<ElementTheme>(value)
{
}