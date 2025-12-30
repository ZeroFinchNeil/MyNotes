using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Content;

using MyNotes.Common.Interop;
using MyNotes.Common.Messages;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Resources;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Views.Notes;

internal sealed partial class NotePage : Page
{
  private readonly NoteViewModel ViewModel;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;

  internal NotePage(NoteWindow noteWindow, Note note)
  {
#if DEBUG
    ReferenceTracker.NotePageReference.Add(this, noteWindow.AppWindow.Id.Value);
#endif
    InitializeComponent();

    var provider = App.Instance.Services.GetRequiredService<NoteViewModelProvider>();
    ViewModel = provider.Resolve(note);
    SettingsService = App.Instance.Services.GetRequiredService<SettingsService>();
    WindowService = App.Instance.Services.GetRequiredService<WindowService>();

    noteWindow.SetTitleBar(NotePage_TitleBarGrid);

    ChangeWindowTheme(ViewModel.Note.Background);

    ChangeFlyoutTheme((ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme));

    RegisterMessengers();

    this.Unloaded += NotePage_Unloaded;
  }

  private void GetWindowInfo(out IntPtr hWnd, out AppWindow? appWindow)
  {
    try
    {
      if (WindowService.NoteWindows.TryGetValue(ViewModel.Note.Id, out var wr)
        && wr.TryGetTarget(out var noteWindow))
      {
        hWnd = WindowNative.GetWindowHandle(noteWindow);
        appWindow = noteWindow.AppWindow;
      }
      else if (this.XamlRoot is XamlRoot xamlRoot
        && xamlRoot.ContentIslandEnvironment is ContentIslandEnvironment env)
      {
        var windowId = env.AppWindowId;
        hWnd = Win32Interop.GetWindowFromWindowId(windowId);
        appWindow = AppWindow.GetFromWindowId(windowId);
      }
      else
      {
        hWnd = IntPtr.Zero;
        appWindow = null;
      }
    }
    catch
    {
      hWnd = IntPtr.Zero;
      appWindow = null;
    }
  }

  private void NotePage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterMessengers();

    ViewModel.Dispose();
    
    Bindings.StopTracking();
  }

  private void NotePage_PinButton_Click(object sender, RoutedEventArgs e)
  {
    GetWindowInfo(out _, out var appWindow);
    var presenter = appWindow?.Presenter as OverlappedPresenter;
    presenter?.IsAlwaysOnTop = !presenter.IsAlwaysOnTop;
  }

  private void NotePage_MinimizeButton_Click(object sender, RoutedEventArgs e)
  {
    GetWindowInfo(out _, out var appWindow);
    var presenter = appWindow?.Presenter as OverlappedPresenter;
    presenter?.Minimize();
  }

  private void NotePage_CloseButton_Click(object sender, RoutedEventArgs e)
  {
    GetWindowInfo(out IntPtr hWnd, out _);
    NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
  }

  private void NotePage_SaveKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    args.Handled = true;
    Console.WriteLine("{0}: {1}", "KeyboardAccelerator", sender.Modifiers + " + " + sender.Key);
    ViewModel.SaveCommand?.Execute();
  }

  private void NotePage_BoldButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Bold = FormatEffect.Toggle;
  }

  private void NotePage_ItalicButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Italic = FormatEffect.Toggle;
  }

  private void NotePage_UnderlineButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Underline = selection.CharacterFormat.Underline == UnderlineType.Single ? UnderlineType.None : UnderlineType.Single;
  }

  private void NotePage_StrikethroughButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Strikethrough = FormatEffect.Toggle;
  }

  private void NotePage_BackgroundColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    ChangeWindowTheme(args.NewColor);
  }

  private void ChangeWindowTheme(Color color)
  {
    color = color.CompositeAlphaWith(Colors.White);

    double preferLight = color.ContrastRatioTo(Colors.Black);
    double preferDark = color.ContrastRatioTo(Colors.White);

    var theme = preferLight >= preferDark ? ElementTheme.Light : ElementTheme.Dark;
    if (this.RequestedTheme != theme)
      this.RequestedTheme = theme;
  }

  private void ChangeFlyoutTheme(ElementTheme theme)
  {
    switch (theme)
    {
      case ElementTheme.Default:
        VisualStateManager.GoToState(this, "FlyoutThemeDefault", false);
        break;
      case ElementTheme.Light:
        VisualStateManager.GoToState(this, "FlyoutThemeLight", false);
        break;
      case ElementTheme.Dark:
        VisualStateManager.GoToState(this, "FlyoutThemeDark", false);
        break;
    }
  }
}

internal sealed partial class NotePage : Page
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, MessageToken>(this, MessageTokens.AppThmeChangedToken, new((recipient, message) => ChangeFlyoutTheme(message.Value)));
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<WindowActivationState>, MessageToken<NoteId>>(this, MessageTokens.NoteWindowActivationChangedToken(ViewModel.Note.Id), new((recipient, message) =>
    {
      if (message.Value == WindowActivationState.Deactivated)
      {
        //_inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, null);
        NotePage_TitleBarGrid.Focus(FocusState.Programmatic);
        VisualStateManager.GoToState(this, "WindowDeactivated", false);
      }
      else
      {
        //SetRegionsForCustomTitleBar();
        NotePage_TitleBarGrid.Focus(FocusState.Programmatic);
        VisualStateManager.GoToState(this, "WindowActivated", false);
      }
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
