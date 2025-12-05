using MyNotes.Models.Navigation;

namespace MyNotes.Views.Windows;

public sealed partial class MainWindowNavigationViewDataTemplateSelector : DataTemplateSelector
{
  public DataTemplate? NavigationCoreNodeTemplate { get; set; }
  public DataTemplate? NavigationSeparatorTemplate { get; set; }
  public DataTemplate? NavigationUserCompositeNodeTemplate { get; set; }
  public DataTemplate? NavigationUserLeafNodeTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationCoreNode => NavigationCoreNodeTemplate,
      NavigationSeparator => NavigationSeparatorTemplate,
      NavigationUserCompositeNode => NavigationUserCompositeNodeTemplate,
      NavigationUserLeafNode => NavigationUserLeafNodeTemplate,
      _ => null
    };
  }
}

public sealed partial class MainWindowTreeViewDataTemplateSelector : DataTemplateSelector
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