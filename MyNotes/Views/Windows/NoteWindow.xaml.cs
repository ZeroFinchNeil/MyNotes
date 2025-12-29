using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Resources;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Windows;

internal sealed partial class NoteWindow : Window
{
  private readonly NoteViewModel ViewModel;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;

  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public NoteWindow(Note note)
  {
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(NoteWindow_TitleBarGrid);

    var provider = App.Instance.Services.GetRequiredService<NoteViewModelProvider>();
    ViewModel = provider.Resolve(note);
    SettingsService = App.Instance.Services.GetRequiredService<SettingsService>();
    WindowService = App.Instance.Services.GetRequiredService<WindowService>();

    WindowService.NoteWindows.Add(note, new WeakReference<NoteWindow>(this));

    // DPI 스케일 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    double scaleFactor = NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    var minimumWindowSize = SettingsDescriptors.NoteWindowMinimumSize.DefaultValue;
    _presenter = AppWindow.Presenter as OverlappedPresenter;
    _presenter?.PreferredMinimumWidth = (int)(minimumWindowSize.Width * scaleFactor);
    _presenter?.PreferredMinimumHeight = (int)(minimumWindowSize.Height * scaleFactor);
    _presenter?.SetBorderAndTitleBar(true, false);

    AppWindow.Resize(new((int)(note.Size.Width * scaleFactor), (int)(note.Size.Height * scaleFactor)));

    // 창 활성화 변경 시
    this.Activated += NoteWindow_Activated;
    AppWindow.Destroying += AppWindow_Destroying;
    AppWindow.Closing += AppWindow_Closing;
    this.Closed += NoteWindow_Closed;
    // 앱 초기 테마 설정
    ChangeWindowTheme(ViewModel.Note.Background);

    // 플라이아웃 테마 설정(전역 테마)
    ChangeFlyoutTheme((ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme));

    // 메신저 등록
    RegisterMessengers();
  }

  private void NoteWindow_Closed(object sender, WindowEventArgs args)
  {
    Console.WriteLine("{0}: {1}", "NoteWindow_Closed", "");
    this.Activated -= NoteWindow_Activated;

    // 메신저 해제
    UnregisterMessengers();

    // 뷰모델 해제
    ViewModel.Dispose();

    // WindowService에서 Window 테이블에서 제거
    //WindowService.NoteWindows.Remove(ViewModel.Note);
  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    Console.WriteLine("{0}: {1}", "AppWindow_Closing", "");
  }

  private void AppWindow_Destroying(AppWindow sender, object args)
  {
    Console.WriteLine("{0}: {1}", "AppWindow_Destroying", "");

    // 바인딩 해제
    Bindings.StopTracking();
  }

  private void NoteWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    if (args.WindowActivationState == WindowActivationState.Deactivated)
    {
      //_inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, null);
      NoteWindow_TitleBarGrid.Focus(FocusState.Programmatic);
      VisualStateManager.GoToState(NoteWindow_RootControl, "WindowDeactivated", false);
    }
    else
    {
      //SetRegionsForCustomTitleBar();
      NoteWindow_TitleBarGrid.Focus(FocusState.Programmatic);
      VisualStateManager.GoToState(NoteWindow_RootControl, "WindowActivated", false);
    }
  }

  private void NoteWindow_PinButton_Click(object sender, RoutedEventArgs e)
  {
    _presenter?.IsAlwaysOnTop = !_presenter.IsAlwaysOnTop;
  }

  private void NoteWindow_MinimizeButton_Click(object sender, RoutedEventArgs e)
  {
    _presenter?.Minimize();
  }

  private void NoteWindow_CloseButton_Click(object sender, RoutedEventArgs e)
  {
    NativeMethods.SendMessage(_hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
  }

  private void NoteWindow_SaveKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    args.Handled = true;
    Console.WriteLine("{0}: {1}", "KeyboardAccelerator", sender.Modifiers + " + " + sender.Key);
    ViewModel.SaveCommand?.Execute();
  }

  private void NoteWindow_BoldButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NoteWindow_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Bold = FormatEffect.Toggle;
  }

  private void NoteWindow_ItalicButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NoteWindow_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Italic = FormatEffect.Toggle;
  }

  private void NoteWindow_UnderlineButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NoteWindow_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Underline = selection.CharacterFormat.Underline == UnderlineType.Single ? UnderlineType.None : UnderlineType.Single;
  }

  private void NoteWindow_StrikethroughButton_Click(object sender, RoutedEventArgs e)
  {
    var selection = NoteWindow_TextEditorRichEditBox.Document.Selection;
    selection.CharacterFormat.Strikethrough = FormatEffect.Toggle;
  }

  private void NoteWindow_BackgroundColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    ChangeWindowTheme(args.NewColor);
  }

  private void ChangeWindowTheme(Color color)
  {
    color = color.CompositeAlphaWith(Colors.White);

    double preferLight = color.ContrastRatioTo(Colors.Black);
    double preferDark = color.ContrastRatioTo(Colors.White);

    var theme = preferLight >= preferDark ? ElementTheme.Light : ElementTheme.Dark;
    if (NoteWindow_RootControl.RequestedTheme != theme)
      NoteWindow_RootControl.RequestedTheme = theme;
  }

  private void ChangeFlyoutTheme(ElementTheme theme)
  {
    switch (theme)
    {
      case ElementTheme.Default:
        VisualStateManager.GoToState(NoteWindow_RootControl, "FlyoutThemeDefault", false);
        break;
      case ElementTheme.Light:
        VisualStateManager.GoToState(NoteWindow_RootControl, "FlyoutThemeLight", false);
        break;
      case ElementTheme.Dark:
        VisualStateManager.GoToState(NoteWindow_RootControl, "FlyoutThemeDark", false);
        break;
    }
  }
}

internal sealed partial class NoteWindow : Window
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, string>(this, MessageTokens.ChangeAppTheme, new((recipient, message) => ChangeFlyoutTheme(message.Value)));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}