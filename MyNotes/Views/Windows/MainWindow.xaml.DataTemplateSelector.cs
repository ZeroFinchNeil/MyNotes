using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Windows;

internal sealed partial class MainWindowNavigationViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationCoreNodeTemplate { get; set; }
  public DataTemplate? NavigationSeparatorTemplate { get; set; }
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      CoreNavigationViewModel => NavigationCoreNodeTemplate,
      SeparatorNavigationViewModel => NavigationSeparatorTemplate,
      UserCompositeNavigationViewModel => NavigationUserCompositeNodeTemplate,
      UserLeafNavigationViewModel => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}

internal sealed partial class MainWindowTreeViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationUserCompositeNode => NavigationUserCompositeNodeTemplate,
      NavigationUserLeafNode => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}