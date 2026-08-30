using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Models.Navigations.Preferences;

namespace MyNotes.Messaging.Messages;

internal sealed class NavigationGroupIconBadgeChangedMessage(GroupIconBadge value) : ValueChangedMessage<GroupIconBadge>(value)
{
}

internal sealed class NavigationTreeChangedMessage()
{
}