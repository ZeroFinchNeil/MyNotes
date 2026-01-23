using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Content;

using MyNotes.Common.Interop;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Models.UI;
using MyNotes.Services.Navigations;
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

  // 생성자
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

    SetEditorText();
    ChangeWindowTheme(ViewModel.Note.Background);

    ChangeFlyoutTheme((ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme));

    RegisterMessengers();

    _editorDebounceTimer.Tick += EditorDebounceTimer_Tick;
    _infoBarDismissTimer.Tick += InfoBarDismissTimer_Tick;

    this.SizeChanged += NotePage_SizeChanged;
    this.Unloaded += NotePage_Unloaded;
  }

  // NoteWindow 접근
  private bool TryGetWindowInfo(out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
  {
    hWnd = IntPtr.Zero;
    appWindow = null;

    try
    {
      if (this.XamlRoot is XamlRoot xamlRoot
        && xamlRoot.ContentIslandEnvironment is ContentIslandEnvironment env)
      {
        var windowId = env.AppWindowId;
        hWnd = Win32Interop.GetWindowFromWindowId(windowId);
        appWindow = AppWindow.GetFromWindowId(windowId);
      }
      else if (WindowService.NoteWindows.TryGetValue(ViewModel.Note.Id, out var wr)
        && wr.TryGetTarget(out var noteWindow))
      {
        hWnd = WindowNative.GetWindowHandle(noteWindow);
        appWindow = noteWindow.AppWindow;
      }
    }
    catch
    { }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  private bool TryExecuteOnWindow(Action<NoteWindow> action)
  {
    if (WindowService.NoteWindows.TryGetValue(ViewModel.Note.Id, out var wr)
        && wr.TryGetTarget(out var noteWindow))
    {
      action.Invoke(noteWindow);
      return true;
    }
    return false;
  }

  // 타이틀 바 드래그 영역 계산
  private void SetRegionsForCustomTitleBar()
  {
    if (TryGetWindowInfo(out _, out var appWindow) && this.XamlRoot is XamlRoot xamlRoot)
    {
      double scaleFactor = xamlRoot.RasterizationScale;

      // 뒤로 가기 버튼, 메뉴 버튼, 검색 상자 영역 위치와 크기 계산
      var PinButtonPosition = NotePage_PinButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, NotePage_PinButton.ActualWidth, NotePage_PinButton.ActualHeight));
      var MoreButtonPosition = NotePage_MoreButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, NotePage_MoreButton.ActualWidth, NotePage_MoreButton.ActualHeight));
      var TitleRenameTextBoxPosition = NotePage_TitleRenameTextBox.Visibility == Visibility.Visible ? NotePage_TitleRenameTextBox.TransformToVisual(null).TransformBounds(new Rect(0, 0, NotePage_TitleRenameTextBox.ActualWidth, NotePage_TitleRenameTextBox.ActualHeight)) : new Rect(0, 0, 0, 0);
      var MinimizeButtonPosition = NotePage_MinimizeButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, NotePage_MinimizeButton.ActualWidth, NotePage_MinimizeButton.ActualHeight));
      var CloseButtonPosition = NotePage_CloseButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, NotePage_CloseButton.ActualWidth, NotePage_CloseButton.ActualHeight));

      RectInt32 PinButtonRect = PinButtonPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 MoreButtonRect = MoreButtonPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 TitleRenameTextBoxRect = TitleRenameTextBoxPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 MinimizeButtonRect = MinimizeButtonPosition.AsScaledRectInt32(scaleFactor);
      RectInt32 CloseButtonRect = CloseButtonPosition.AsScaledRectInt32(scaleFactor);

      // 제목 표시줄 드래그 제외할 영역 설정
      var _inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(appWindow.Id);
      _inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, [PinButtonRect, MoreButtonRect, TitleRenameTextBoxRect, MinimizeButtonRect, CloseButtonRect]);
    }
  }

  // 본문
  private void SetEditorText()
  {
    var rtfText = ViewModel.Note.Body;
    if (string.IsNullOrEmpty(rtfText))
    {
      NotePage_TextEditorRichEditBox.RequestedTheme = GetThemeForColor(ViewModel.Note.Background);
    }
    else
    {
      NotePage_TextEditorRichEditBox.Document.SetText(TextSetOptions.FormatRtf, ViewModel.Note.Body);
    }
  }

  // 테마 관련
  private void ChangeWindowTheme(Color color)
  {
    var theme = GetThemeForColor(color);
    if (this.RequestedTheme != theme)
      this.RequestedTheme = theme;
  }

  private ElementTheme GetThemeForColor(Color color)
  {
    color = color.CompositeAlphaWith(Colors.White);

    double preferLight = color.ContrastRatioTo(Colors.Black);
    double preferDark = color.ContrastRatioTo(Colors.White);

    return preferLight >= preferDark ? ElementTheme.Light : ElementTheme.Dark;
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

  // 본문 텍스트 변경 시 뷰모델 및 DB 업데이트
  private void UpdateEditorBodyText()
  {
    NotePage_TextEditorRichEditBox.Document.GetText(TextGetOptions.FormatRtf, out var editorText);
    editorText = Regexes.LastParInRtfRegex().Replace(editorText, "}");
    NotePage_TextEditorRichEditBox.Document.GetText(TextGetOptions.None, out var plainText);
    ViewModel.Note.Body = editorText;
    ViewModel.Note.BodyPlainText = plainText;

    if (_changePreview)
    {
      ViewModel.Preview = ViewModel.GetPreview(ViewModel.Note.Body, 0, _previewTextMaxLength);
      _changePreview = false;
    }
  }

  private void NotePage_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    if (FocusManager.GetFocusedElement(XamlRoot) is FrameworkElement focusedElement
      && focusedElement == NotePage_TextEditorRichEditBox)
    {
      NotePage_TitleBarGrid.Focus(FocusState.Programmatic);
    }
  }

  private void NotePage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterMessengers();

    _editorDebounceTimer.Tick -= EditorDebounceTimer_Tick;
    UpdateEditorBodyText();

    var navigationService = App.Instance.Services.GetRequiredService<NavigationService>();
    if (!(navigationService.CurrentNavigation is NavigationUserLeafNode navigation
          && navigation.Id != ViewModel.Note.NavigationId))
    {
      ViewModel.Dispose();
    }

    Bindings.StopTracking();
  }
}

#region 상단 타이틀 바 영역
internal sealed partial class NotePage : Page
{
  // 타이틀바 드래그 영역 조정(로드 및 크기 변경 시)
  private void NotePage_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void NotePage_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void NotePage_PinButton_Click(object sender, RoutedEventArgs e)
  {
    if (TryGetWindowInfo(out _, out var appWindow))
    {
      var presenter = appWindow?.Presenter as OverlappedPresenter;
      presenter?.IsAlwaysOnTop = !presenter.IsAlwaysOnTop;
    }
  }

  private void NotePage_MinimizeButton_Click(object sender, RoutedEventArgs e)
  {
    if (TryGetWindowInfo(out _, out var appWindow))
    {
      var presenter = appWindow?.Presenter as OverlappedPresenter;
      presenter?.Minimize();
    }
  }

  private void NotePage_CloseButton_Click(object sender, RoutedEventArgs e)
  {
    if (TryGetWindowInfo(out IntPtr hWnd, out _))
    {
      NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
    }
  }

  private void NotePage_RenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "TitleBarTitleRename", false);
    NotePage_TitleRenameTextBox.Focus(FocusState.Keyboard);
    NotePage_TitleRenameTextBox.LayoutUpdated += NotePage_TitleRenameTextBox_LayoutUpdated;
  }

  private void NotePage_TitleRenameTextBox_LayoutUpdated(object? sender, object e)
  {
    NotePage_TitleRenameTextBox.LayoutUpdated -= NotePage_TitleRenameTextBox_LayoutUpdated;
    SetRegionsForCustomTitleBar();
  }

  private void NotePage_TitleRenameTextBox_LostFocus(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "TitleBarTitleNormal", false);
    NotePage_TitleRenameTextBox.LayoutUpdated += NotePage_TitleRenameTextBox_LayoutUpdated;
  }
}
#endregion

#region 에디터 영역
internal sealed partial class NotePage : Page
{
  private readonly DispatcherTimer _editorDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(2000) };

  private void EditorDebounceTimer_Tick(object? sender, object e)
  {
    _editorDebounceTimer.Stop();
    UpdateEditorBodyText();
  }

  private static readonly byte _editorDebounceCountThreshold = 20;
  private byte _editorDebounceCount = 0;
  private void NotePage_TextEditorRichEditBox_TextChanged(object sender, RoutedEventArgs e)
  {
    if (!IsLoaded)
      return;

    _editorDebounceTimer.Stop();
    _editorDebounceTimer.Start();

    if (_editorDebounceCount++ >= _editorDebounceCountThreshold)
    {
      UpdateEditorBodyText();
      _editorDebounceCount = 0;
    }
  }

  private int _previousSelectionIndex = 0;
  private int _currentSelectionIndex = 0;
  private void NotePage_TextEditorRichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
  {
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;
    _previousSelectionIndex = selection.GetIndex(0);

    var characterFormat = selection.CharacterFormat;
    NotePage_BoldButton.IsChecked = characterFormat.Bold is FormatEffect.On;
    NotePage_ItalicButton.IsChecked = characterFormat.Italic is FormatEffect.On;
    NotePage_UnderlineButton.IsChecked = characterFormat.Underline is UnderlineType.Single;
    NotePage_StrikethroughButton.IsChecked = characterFormat.Strikethrough is FormatEffect.On;
    NotePage_FontSizeComboBox.Text = characterFormat.Size > 0 ? characterFormat.Size.ToString() : string.Empty;
  }

  private bool _changePreview = false;
  private readonly int _previewTextMaxLength = 100;
  private void NotePage_TextEditorRichEditBox_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
  {
    _currentSelectionIndex = NotePage_TextEditorRichEditBox.Document.Selection.GetIndex(0);
    _changePreview = _previousSelectionIndex <= _previewTextMaxLength && _currentSelectionIndex <= _previewTextMaxLength;
  }
}
#endregion

#region 하단 커맨드 바 영역
internal sealed partial class NotePage : Page
{
  private void NotePage_BoldButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.Bold = FormatEffect.Toggle;
  }

  private void NotePage_ItalicButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.Italic = FormatEffect.Toggle;
  }

  private void NotePage_UnderlineButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.Underline = characterFormat.Underline == UnderlineType.Single ? UnderlineType.None : UnderlineType.Single;
  }

  private void NotePage_StrikethroughButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.Strikethrough = FormatEffect.Toggle;
  }

  private void NotePage_BackgroundColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    ChangeWindowTheme(args.NewColor);
  }

  private void NotePage_BackdropRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    TryExecuteOnWindow((noteWindow) =>
    {
      noteWindow.SystemBackdrop = (BackdropKind)(NotePage_BackdropRadioButtons.SelectedIndex) switch
      {
        BackdropKind.Acrylic => new DesktopAcrylicBackdrop(),
        BackdropKind.Mica => new MicaBackdrop(),
        BackdropKind.None or _ => null
      };
    });
  }

  private void NotePage_FontColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.ForegroundColor = args.NewColor;
  }

  private void NotePage_HighlightColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.BackgroundColor = args.NewColor;
  }

  private void NotePage_FontColorButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.ForegroundColor = NotePage_FontColorPicker.Color;
  }

  private void NotePage_HighlightButton_Click(object sender, RoutedEventArgs e)
  {
    var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
    characterFormat.BackgroundColor = NotePage_HighlightColorPicker.Color;
  }

  private readonly ImmutableList<float> EditorFontSizes = [8, 9, 10.5f, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48];
  private void NotePage_FontSizeComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs e)
  {
    if (e.Text.Length <= 5
      && float.TryParse(e.Text, out float fontSize)
      && ValidateEditorFontSize(fontSize))
    {
      var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
      characterFormat.Size = fontSize;
    }

    e.Handled = true;
  }

  private void NotePage_DecreaseFontSizeButton_Click(object sender, RoutedEventArgs e)
  {
    string text = NotePage_FontSizeComboBox.Text;
    if (text.Length <= 5 && float.TryParse(text, out float num))
    {
      var fontSize = GetEditorFontSizeLowerBound(num);
      if (ValidateEditorFontSize(fontSize))
      {
        var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
        characterFormat.Size = fontSize;
        NotePage_FontSizeComboBox.Text = $"{fontSize}";
      }
    }
  }

  private void NotePage_IncreaseFontSizeButton_Click(object sender, RoutedEventArgs e)
  {
    string text = NotePage_FontSizeComboBox.Text;
    if (text.Length <= 5 && float.TryParse(text, out float num))
    {
      var fontSize = GetEditorFontSizeUpperBound(num);
      if (ValidateEditorFontSize(fontSize))
      {
        var characterFormat = NotePage_TextEditorRichEditBox.Document.Selection.CharacterFormat;
        characterFormat.Size = fontSize;
        NotePage_FontSizeComboBox.Text = $"{fontSize}";
      }
    }
  }

  private static readonly float _minEditorFontSize = 5.0f;
  private static readonly float _maxEditorFontSize = 512.0f;

  private static bool ValidateEditorFontSize(float fontSize)
  {
    if (fontSize >= _minEditorFontSize && fontSize <= _maxEditorFontSize)
    {
      float eps = 1e-6f;
      float truncated = (float)Math.Truncate(fontSize * 100) / 100f;
      float fraction = Math.Abs(fontSize - (float)Math.Floor(fontSize));

      if (Math.Abs(fraction - 0.0f) < eps || Math.Abs(fraction - 0.5f) < eps)
      {
        if (Math.Abs(fontSize - truncated) < eps)
        {
          return true;
        }
      }
    }
    return false;
  }

  private static float GetEditorFontSizeUpperBound(float fontSize) => fontSize switch
  {
    >= 5 and < 12 => fontSize + 0.5f,
    < 20 => fontSize + 1.0f,
    < 32 => fontSize.GreaterThanNearestMultiple(2),
    < 64 => fontSize.GreaterThanNearestMultiple(4),
    < 512 => fontSize.GreaterThanNearestMultiple(8),
    512 => 512,
    _ => 0
  };

  private static float GetEditorFontSizeLowerBound(float fontSize) => fontSize switch
  {
    5 => 5,
    > 5 and <= 12 => fontSize - 0.5f,
    <= 20 => fontSize - 1.0f,
    <= 32 => fontSize.LessThanNearestMultiple(2),
    <= 64 => fontSize.LessThanNearestMultiple(4),
    <= 512 => fontSize.LessThanNearestMultiple(8),
    _ => 0
  };

}
#endregion

#region Keyboard Accelerators
internal sealed partial class NotePage : Page
{
  private readonly DispatcherTimer _infoBarDismissTimer = new() { Interval = TimeSpan.FromMilliseconds(2000) };

  private void OpenInfoBar()
  {
    NotePage_InfoBar.IsOpen = true;
    _infoBarDismissTimer.Start();
  }

  private void InfoBarDismissTimer_Tick(object? sender, object e)
  {
    NotePage_InfoBar.IsOpen = false;
    _infoBarDismissTimer.Stop();
  }

  private async void NotePage_SaveKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    args.Handled = true;
    UpdateEditorBodyText();
    await ViewModel.UpdateNoteEntity();
    NotePage_InfoBar.Title = "Saved";
    NotePage_InfoBar.Severity = InfoBarSeverity.Success;
    OpenInfoBar();
  }

  private void NotePage_FindKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    if (NotePage_FindReplaceBox.IsOpen)
      VisualStateManager.GoToState(this, "EditorSearchNone", false);
    else
      VisualStateManager.GoToState(this, "EditorSearching", false);
  }

  private void NotePage_ReplaceKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    if (NotePage_FindReplaceBox.IsOpen)
      VisualStateManager.GoToState(this, "EditorSearchNone", false);
    else
      VisualStateManager.GoToState(this, "EditorSearching", false);
  }

  private void NotePage_RenameTitleKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    VisualStateManager.GoToState(this, "TitleBarTitleRename", false);
    NotePage_TitleRenameTextBox.Focus(FocusState.Keyboard);
    NotePage_TitleRenameTextBox.LayoutUpdated += NotePage_TitleRenameTextBox_LayoutUpdated;
  }
}
#endregion

#region 메신저 및 커맨드
internal sealed partial class NotePage : Page
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, MessageToken>(this, MessageTokens.AppThmeChangedToken, new((recipient, message) => ChangeFlyoutTheme(message.Value)));

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<WindowPresenterState>, MessageToken<NoteId>>(this, MessageTokens.NoteWindowActivationChangedToken(ViewModel.Note.Id), new((recipient, message) =>
    {
      WindowPresenterState state = message.Value;
      WindowActivationState windowState = state.WindowActivationState;
      OverlappedPresenterState presenterState = state.OverlappedPresenterState;

      NotePage_TitleBarGrid.Focus(FocusState.Programmatic);
      if (windowState is WindowActivationState.Deactivated)
      {
        if (presenterState is OverlappedPresenterState.Maximized)
        {
          VisualStateManager.GoToState(this, "WindowDeactivatedMaximized", false);
        }
        else
        {
          VisualStateManager.GoToState(this, "WindowDeactivated", false);
        }
      }
      else
      {
        VisualStateManager.GoToState(this, "WindowActivated", false);
      }
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
#endregion