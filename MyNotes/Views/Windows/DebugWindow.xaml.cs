using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Services.Windows;

namespace MyNotes.Views.Windows;

internal sealed partial class DebugWindow : Window
{
  public DebugWindow()
  {
    InitializeComponent();
    double scale = DebugWindow_RootGrid.XamlRoot?.RasterizationScale ?? 1.0;
    this.AppWindow.MoveAndResize(new(0, 0, (int)(800 * scale), (int)(950 * scale)));
  }

  private async void DebugWindow_SeparatorButton_Click(object sender, RoutedEventArgs e) => PrintSeparator();

  private void DebugWindow_GCButton_Click(object sender, RoutedEventArgs e) => ExecuteGC();

  private static void PrintSeparator()
  {
    ConsoleHelper.WriteLine(true);
    ConsoleHelper.WriteLine(true, "-------------------------------------------------------------------------------");
    ConsoleHelper.WriteLine(true);
  }

  private static void ExecuteGC()
  {
    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
    GC.WaitForPendingFinalizers();
    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
  }

  public ObservableCollection<IGrouping<string?, DebugWindowReferenceItemGroup>> GroupItems { get; } = new();
  public ObservableCollection<DebugWindowReferenceItemPropertyInfo> ReferencePropertyInfo { get; } = new();

  private static readonly ImmutableList<string> GroupNames = ["NavigationViewModel", "NoteViewModel", "ViewModel", "Container", "Dialog", "Page", "Window", "Navigation", "Note"];

  private void DebugWindow_ShowReferencesButton_Click(object sender, RoutedEventArgs e)
  {
    DebugWindow_ShowReferencesButton.IsEnabled = false;
    ExecuteGC();

    var references = ReferenceTracker.GetAliveReferences();
    List<DebugWindowReferenceItemGroup> itemGroups = new();
    var group = references
      .GroupBy(pair => GroupNames.FirstOrDefault(name => pair.Key.Name.Contains(name)) ?? "Object");

    foreach (var g in group)
    {
      foreach (var pair in g.OrderBy(pair => pair.Key.Name))
      {
        List<DebugWindowReferenceItem> items = new();
        var properties = pair.Key.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var events = pair.Key.GetEvents(BindingFlags.Public | BindingFlags.Instance);
        foreach (var obj in pair.Value)
        {
          if (obj is null)
          {
            continue;
          }

          DebugWindowReferenceItem item = new() { Type = pair.Key.Name, HashCode = obj.GetHashCode() };
          try
          {
            foreach (var property in properties)
            {
              var indexParams = property.GetIndexParameters();
              if (indexParams.Length == 0)
              {
                item.PropertyInfo.Add(new() { Name = property.Name, Value = property.GetValue(obj)?.ToString() ?? string.Empty });
              }
              else if (indexParams.First() is ParameterInfo parameterInfo && parameterInfo.ParameterType.Equals(typeof(int)))
              {
                int index = 0;
                List<object> values = new();
                while (index <= 10)
                {
                  try
                  {
                    var v = property.GetValue(obj, [index++]);
                    if (v is null)
                    {
                      break;
                    }
                    values.Add(v);
                  }
                  catch
                  {
                    break;
                  }
                }

                item.PropertyInfo.Add(new() { Name = property.Name, Value = string.Join(", ", values) });
              }
              else
              {
                item.PropertyInfo.Add(new() { Name = property.Name, Value = string.Empty });
              }
            }

            foreach (var ev in events)
            {
              try
              {
                if (pair.Key.GetField(ev.Name)?.GetValue(obj) is Delegate handler)
                {
                  item.PropertyInfo.Add(new() { Name = $"{ev.Name} (event)", Value = string.Join(", ", handler.GetInvocationList().Select(h => h.Method.Name)) });
                }
              }
              catch
              { }
            }
          }
          catch
          { }
          items.Add(item);
        }

        itemGroups.Add(new() { GroupName = g.Key, Type = pair.Key.Name, Descriptions = items });
      }
    }

    GroupItems.Clear();
    foreach (var i in itemGroups.GroupBy(item => item.GroupName).OrderBy(g => g.Key))
    {
      GroupItems.Add(i);
    }

    DebugWindow_ShowReferencesButton.IsEnabled = true;
  }

  private async void DebugWindow_MainWindowButton_Click(object sender, RoutedEventArgs e)
  {
    var mainWindowService = App.Services.GetRequiredService<MainWindowService>();
    if (mainWindowService.TryGetCurrentWindow(out var mainWindow))
    {
      mainWindow.Activate();
    }
  }

  private void DebugWindow_AlwaysOnTopToggleButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.AppWindow.Presenter is OverlappedPresenter presenter)
    {
      presenter.IsAlwaysOnTop = DebugWindow_AlwaysOnTopToggleButton.IsChecked ?? false;
    }
  }

  private void DebugWindow_DescriptionButton_Click(object sender, RoutedEventArgs e)
  {
    if (sender is Button button && button.DataContext is DebugWindowReferenceItem item)
    {
      ReferencePropertyInfo.Clear();
      DebugWindow_ReferencePropertyInfosListView.Header = $"[ {item.Type} ]\r\n( {item.HashCode} )";
      foreach (var info in item.PropertyInfo)
      {
        ReferencePropertyInfo.Add(info);
      }
    }
  }
}

internal record DebugWindowReferenceItemGroup
{
  public string? GroupName { get; set; }
  public string? Type { get; set; }

  public IReadOnlyList<DebugWindowReferenceItem>? Descriptions { get; set; }
}

internal record DebugWindowReferenceItem
{
  public string Type { get; set; } = string.Empty;

  public int HashCode { get; set; }

  public List<DebugWindowReferenceItemPropertyInfo> PropertyInfo { get; set; } = new();
}

internal record DebugWindowReferenceItemPropertyInfo
{
  public string Name { get; set; } = string.Empty;

  public string Value { get; set; } = string.Empty;
}