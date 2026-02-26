namespace MyNotes.Models.Navigations;

internal interface INavigation
{
}

//INavigation
//├─ INavigationNode
//│  ├─ NavigationCoreNode
//│  │  ├─ NavigationHome ──────────────────────────────────────┐              
//│  │  ├─ NavigationBookmarks ────────┐───── INavigationInitialTarget
//│  │  ├─ NavigationTrash             │                        │
//│  │  └─ NavigationSettings          │                        │
//│  └─ NavigationUserNode             ├─ INavigationNoteList   │
//│     ├─ NavigationUserCompositeNode │                        │
//│     │  └─ NavigationUserRootNode   │                        │
//│     └─ NavigationUserLeafNode ─────┤────────────────────────┘
//├─ NavigationSearch ─────────────────┘
//└─ NavigationSeparator