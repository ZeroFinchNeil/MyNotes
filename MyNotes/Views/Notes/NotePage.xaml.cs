using System.Runtime.InteropServices;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.Storage.Pickers;

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
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Windows;

using Windows.Storage.Streams;
using Windows.System;

namespace MyNotes.Views.Notes;

internal sealed partial class NotePage : Page
{
  private readonly NoteViewModel ViewModel;
  private readonly NoteEditorViewModel EditorViewModel;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;

  // 생성자
  internal NotePage(NoteWindow noteWindow, Note note)
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.PageReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif
    InitializeComponent();

    var noteViewModelProvider = App.Services.GetRequiredService<NoteViewModelProvider>();
    var editorViewModelProvider = App.Services.GetRequiredService<NoteEditorViewModelProvider>();
    ViewModel = noteViewModelProvider.Resolve(note);
    EditorViewModel = editorViewModelProvider.Resolve(note, NotePage_TextEditorRichEditBox.Document);
    SettingsService = App.Services.GetRequiredService<SettingsService>();
    WindowService = App.Services.GetRequiredService<WindowService>();
    noteWindow.SetTitleBar(NotePage_TitleBarGrid);

    SetEditorText();

    ChangeFlyoutTheme((ElementTheme)SettingsService.Load(SettingsDescriptors.AppTheme));

    RegisterMessengers();

    _infoBarDismissTimer.Tick += InfoBarDismissTimer_Tick;

    this.SizeChanged += NotePage_SizeChanged;
    this.Loaded += NotePage_Loaded;
    this.Unloaded += NotePage_Unloaded;
    noteWindow.AppWindow.Closing += AppWindow_Closing;
  }

  private void NotePage_Loaded(object sender, RoutedEventArgs e)
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out var hWnd, out var appWindow))
    {
      appWindow.Changed += AppWindow_Changed;

      ViewModel.Note.IsWindowOpen = true;

      _newWndProcCallback = (handle, msg, wParam, lParam) =>
      {
        // 시스템에 의한 종료 시 창 복원을 위해 창 닫힘을 기록하지 않음
        switch (msg)
        {
          case (uint)NativeMethods.WindowMessage.WM_CLOSE:
            break;
          case (uint)NativeMethods.WindowMessage.WM_QUERYENDSESSION:
            _isManualClose = false;
            break;
        }

        // 기존 wndProc 호출
        return NativeMethods.CallWindowProc(_oldWndProc, handle, msg, wParam, lParam);
      };

      _newWndProc = Marshal.GetFunctionPointerForDelegate(_newWndProcCallback);
      _oldWndProc = NativeMethods.SetWindowLongPtr(hWnd, GWLP_WNDPROC, _newWndProc);
    }
  }

  private void NotePage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterMessengers();
    EditorViewModel.UpdateEditorBodyText();
    if (EditorViewModel.ShouldChangePreview)
    {
      ViewModel.Preview = ViewModel.GetPreview(ViewModel.Note.Body, 0, EditorViewModel.PreviewTextMaxLength);
      EditorViewModel.ShouldChangePreview = false;
    }

    var navigationService = App.Services.GetRequiredService<NavigationService>();
    if (!(navigationService.CurrentNavigation is NavigationUserLeafNode navigation
          && navigation.Id != ViewModel.Note.NavigationId))
    {
      ViewModel.Dispose();
    }
    EditorViewModel.Dispose();

    Bindings.StopTracking();
  }

  // 타이틀 바 드래그 영역 계산
  private void SetRegionsForCustomTitleBar()
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out _, out var appWindow) && this.XamlRoot is XamlRoot xamlRoot)
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
    if (!string.IsNullOrEmpty(rtfText))
    {
      NotePage_TextEditorRichEditBox.Document.SetText(TextSetOptions.FormatRtf, ViewModel.Note.Body);
    }
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

  private void NotePage_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    if (FocusManager.GetFocusedElement(XamlRoot) is FrameworkElement focusedElement
      && focusedElement == NotePage_TextEditorRichEditBox)
    {
      NotePage_TitleBarGrid.Focus(FocusState.Programmatic);
    }
  }

  private IntPtr _oldWndProc = IntPtr.Zero;
  private IntPtr _newWndProc = IntPtr.Zero;
  private NativeMethods.WndProcCallback? _newWndProcCallback;
  private readonly int GWLP_WNDPROC = -4;
  private bool _isManualClose = true;

  private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
  {
    if (args.DidSizeChange)
    {
      if (sender.Presenter is OverlappedPresenter presenter
        && presenter.State is OverlappedPresenterState.Restored)
      {
        ViewModel.Note.Size = sender.Size;
      }
    }
    else if (args.DidPositionChange)
    {
      if (sender.Presenter is OverlappedPresenter presenter
        && presenter.State is OverlappedPresenterState.Restored)
      {
        ViewModel.Note.Position = sender.Position;
      }
    }
  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    sender.Changed -= AppWindow_Changed;
    sender.Closing -= AppWindow_Closing;

    if (_isManualClose)
      ViewModel.Note.IsWindowOpen = false;

    IntPtr hWnd = Win32Interop.GetWindowFromWindowId(sender.Id);
    if (hWnd != IntPtr.Zero)
    {
      // 원래 WndProc으로 복귀
      _ = NativeMethods.SetWindowLongPtr(hWnd, GWLP_WNDPROC, _oldWndProc);
    }
    _newWndProcCallback = null;
  }

  private void NotePage_ViewModeRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      switch (item.Tag)
      {
        case string tag when tag is "Edit":
          VisualStateManager.GoToState(this, "ViewModeEdit", false);
          break;
        case string tag when tag is "ReadOnly":
          VisualStateManager.GoToState(this, "ViewModeReadOnly", false);
          break;
      }
    }
  }

  private async void NotePage_SaveAsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is MenuFlyoutItem item
      && WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out _, out var appWindow))
    {
      (string Extension, string Kind)? fileType = item.Tag switch
      {
        string tag when tag is "SaveAsPlainText" => (".txt", "text"),
        string tag when tag is "SaveAsRichText" => (".rtf", "rich text"),
        string tag when tag is "SaveAsPDF" => (".pdf", "PDF"),
        _ => null
      };

      if (fileType is not null)
      {
        string suggestedFileName = ViewModel.Note.Title;
        foreach (var ch in System.IO.Path.GetInvalidFileNameChars())
        {
          suggestedFileName = suggestedFileName.Replace(ch, '_');
        }
        if (string.IsNullOrEmpty(suggestedFileName))
          suggestedFileName = $"MyNote_{DateTime.UtcNow:yyyyMMdd_hhmmss}";

        FileSavePicker picker = new(appWindow.Id)
        {
          SuggestedFileName = suggestedFileName,
          SuggestedStartLocation = PickerLocationId.Desktop
        };
        picker.FileTypeChoices.Add(item.Text, [fileType.Value.Extension]);
        picker.DefaultFileExtension = fileType.Value.Extension;

        if (await picker.PickSaveFileAsync() is PickFileResult result)
        {
          string savePath = result.Path;
          switch (fileType.Value.Extension)
          {
            case string ex when ex is ".txt":
              NotePage_TextEditorRichEditBox.Document.GetText(TextGetOptions.None, out var plainText);
              await File.WriteAllTextAsync(savePath, plainText);
              break;
            case string ex when ex is ".rtf":
              var rtfFile = await StorageFile.GetFileFromPathAsync(savePath);
              using (IRandomAccessStream randAccStream = await rtfFile.OpenAsync(FileAccessMode.ReadWrite))
              {
                NotePage_TextEditorRichEditBox.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
              }
              break;
          }
          Button actionButton = new()
          {
            Content = "Show in folder"
          };

          async void SaveAsInfoBarActionButton_Click(object sender, RoutedEventArgs e)
          {
            actionButton.Click -= SaveAsInfoBarActionButton_Click;
            NotePage_InfoBar.ActionButton = null;
            var folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(savePath));
            await Launcher.LaunchFolderAsync(folder);
            NotePage_InfoBar.IsOpen = false;
          }
          actionButton.Click += SaveAsInfoBarActionButton_Click;
          NotePage_InfoBar.Title = $"Saved as a {fileType.Value.Kind} file.";
          NotePage_InfoBar.ActionButton = actionButton;
          NotePage_InfoBar.Severity = InfoBarSeverity.Success;
          OpenInfoBar(interval: TimeSpan.FromSeconds(7), showCloseButtonOnAutoClose: true,
            actionAfterAutoClosed: () =>
            {
              actionButton.Click -= SaveAsInfoBarActionButton_Click;
              NotePage_InfoBar.ActionButton = null;
            });
        }
      }
    }
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
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out _, out var appWindow))
    {
      var presenter = appWindow?.Presenter as OverlappedPresenter;
      presenter?.IsAlwaysOnTop = !presenter.IsAlwaysOnTop;
    }
  }

  private void NotePage_MinimizeButton_Click(object sender, RoutedEventArgs e)
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out _, out var appWindow))
    {
      var presenter = appWindow?.Presenter as OverlappedPresenter;
      presenter?.Minimize();
    }
  }

  private void NotePage_CloseButton_Click(object sender, RoutedEventArgs e)
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out IntPtr hWnd, out _))
    {
      NativeMethods.SendMessage(hWnd, (uint)NativeMethods.WindowMessage.WM_SYSCOMMAND, (IntPtr)NativeMethods.SystemCommand.SC_CLOSE, IntPtr.Zero);
    }
  }

  private void NotePage_RenameTitleMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "TitleBarTitleRename", false);
    NotePage_TitleRenameTextBox.Focus(FocusState.Keyboard);
    NotePage_TitleRenameTextBox.SelectAll();
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
  private void NotePage_BackdropRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    WindowService.TryExecuteOnNoteWindow(ViewModel.Note.Id, (noteWindow) =>
    {
      noteWindow.SystemBackdrop = (BackdropKind)NotePage_BackdropRadioButtons.SelectedIndex switch
      {
        BackdropKind.Acrylic => new DesktopAcrylicBackdrop(),
        BackdropKind.Mica => new MicaBackdrop(),
        BackdropKind.None or _ => null
      };
    });
  }

  private bool IsDefaultColor(Color color) => color.A < 255;
}
#endregion

#region Keyboard Accelerators
internal sealed partial class NotePage : Page
{
  private readonly DispatcherTimer _infoBarDismissTimer = new() { Interval = TimeSpan.FromSeconds(2) };

  private void OpenInfoBar(TimeSpan? interval = null, bool showCloseButtonOnAutoClose = false, Action? actionAfterAutoClosed = null)
  {
    NotePage_InfoBar.IsOpen = true;

    if (interval is null)
    {
      NotePage_InfoBar.IsClosable = true;
    }
    else
    {
      NotePage_InfoBar.IsClosable = showCloseButtonOnAutoClose;
      _infoBarDismissTimer.Stop();
      _infoBarDismissTimer.Interval = interval.Value;
      void InfoBarDismissTimer_Tick_WhenAutoClosed(object? sender, object e)
      {
        _infoBarDismissTimer.Tick -= InfoBarDismissTimer_Tick_WhenAutoClosed;
        actionAfterAutoClosed?.Invoke();
      }
      if (actionAfterAutoClosed is not null)
        _infoBarDismissTimer.Tick += InfoBarDismissTimer_Tick_WhenAutoClosed;
      _infoBarDismissTimer.Start();
    }
  }

  private void InfoBarDismissTimer_Tick(object? sender, object e)
  {
    NotePage_InfoBar.IsOpen = false;
    _infoBarDismissTimer.Stop();
  }

  private async void NotePage_SaveKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    args.Handled = true;
    EditorViewModel.UpdateEditorBodyText();
    if (EditorViewModel.ShouldChangePreview)
    {
      ViewModel.Preview = ViewModel.GetPreview(ViewModel.Note.Body, 0, EditorViewModel.PreviewTextMaxLength);
      EditorViewModel.ShouldChangePreview = false;
    }
    await ViewModel.UpdateNoteEntity();
    NotePage_InfoBar.Title = "Saved";
    NotePage_InfoBar.ActionButton = null;
    NotePage_InfoBar.Severity = InfoBarSeverity.Success;
    OpenInfoBar(TimeSpan.FromSeconds(2));
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