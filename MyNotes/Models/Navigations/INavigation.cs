namespace MyNotes.Models.Navigations;

internal interface INavigation { }

// INavigation
// ├─ INavigationNode
// │    ├─ NavigationCoreNode
// │    │    └─ NavigationHome, NavigationBookmarks,      
// │    │       NavigationTrash, NavigationSettings
// │    └─ NavigationUserNode
// │         ├─ NavigationUserCompositeNode
// │         │    └─ NavigationUserRootNode
// │         └─ NavigationUserLeafNode
// └─ NavigationSeparator