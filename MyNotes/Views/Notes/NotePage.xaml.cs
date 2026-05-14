using System.Runtime.InteropServices;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.Storage.Pickers;
using MyNotes.Common.Interop;
using MyNotes.Common.Messages;
using MyNotes.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Models.UI;
using MyNotes.Services.App;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Notes;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.ViewModels.Notes;
using MyNotes.ViewModels.Notes.Providers;
using MyNotes.Views.Windows;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.System;
using MyNotes.Shared.Constants;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Enums.Notes;

namespace MyNotes.Views.Notes;

[Debugging.ReferenceTracker]
internal sealed partial class NotePage : Page
{
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly NoteEditorViewModelProvider NoteEditorViewModelProvider;

  private readonly NoteViewModel ViewModel;
  private readonly NoteEditorViewModel EditorViewModel;
  private readonly ImageCollectionViewModel ImageCollectionViewModel;
  private readonly SettingsService SettingsService;
  private readonly WindowService WindowService;

  #region Object Lifetime Management
  internal NotePage(NoteWindow noteWindow, NoteModel note)
  {
    TrackReference();
    InitializeComponent();

    NoteViewModelProvider = App.Services.GetRequiredService<NoteViewModelProvider>();
    NoteEditorViewModelProvider = App.Services.GetRequiredService<NoteEditorViewModelProvider>();

    var imageCollectionViewModelProvider = App.Services.GetRequiredService<ImageCollectionViewModelProvider>();
    ViewModel = NoteViewModelProvider.Resolve(note);
    EditorViewModel = NoteEditorViewModelProvider.Resolve(note, NotePage_TextEditorRichEditBox.Document);

    var noteService = App.Services.GetRequiredService<NoteService>();
    ImageCollectionViewModel = imageCollectionViewModelProvider.Resolve(noteService.CreateImageCollectionKey(note));
    var imageViewModels = ImageCollectionViewModel.ImageViewModels;
    ViewModel.IsImagePanelVisible = imageViewModels.Count > 0;
    imageViewModels.CollectionChanged += ImageViewModels_CollectionChanged;

    SettingsService = App.Services.GetRequiredService<SettingsService>();
    WindowService = App.Services.GetRequiredService<WindowService>();
    noteWindow.SetTitleBar(NotePage_TitleBarGrid);

    SetEditorText();

    ChangeFlyoutTheme((ElementTheme)SettingsService.Load(AppSettingsDescriptors.AppTheme));

    RegisterMessengers();

    _infoBarDismissTimer.Tick += InfoBarDismissTimer_Tick;

    this.SizeChanged += NotePage_SizeChanged;
    this.Loaded += NotePage_Loaded;
    this.Unloaded += NotePage_Unloaded;
    noteWindow.AppWindow.Closing += AppWindow_Closing;
  }

  private async void NotePage_Loaded(object sender, RoutedEventArgs e)
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out var hWnd, out var appWindow))
    {
      appWindow.Changed += AppWindow_Changed;

      ViewModel.Note.IsWindowOpen = true;
      (appWindow.Presenter as OverlappedPresenter)?.IsAlwaysOnTop = ViewModel.Note.IsAlwaysOnTop;

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

      if (ViewModel.Note.NavigationId == NavigationId.Empty)
      {
        var dialogService = App.Services.GetRequiredService<DialogService>();
        var noteListViewModelProvider = App.Services.GetRequiredService<NoteListViewModelProvider>();
        var result = await dialogService.ShowSelectNoteParentDialogAsync(XamlRoot);
        var contentDialogResult = result.ContentDialogResult;
        switch (contentDialogResult)
        {
          case ContentDialogResult.Primary:
            if (result.navigationId is NavigationId parentId && parentId != NavigationId.Empty)
            {
              ViewModel.Note.NavigationId = parentId;

              if (noteListViewModelProvider.TryResolve(parentId, out var noteListViewModel)
                && noteListViewModel.NoteViewModels is NoteViewModelCollection noteViewModels
                && !noteViewModels.Contains(ViewModel))
              {
                noteViewModels.Add(ViewModel);
              }
            }
            break;
          case ContentDialogResult.None:
            ViewModel.CloseWindowCommand.Execute(ViewModel.Note);
            break;
        }
      }
    }

    ViewModel.ChangeNoteBackdrop();
  }

  private async void NotePage_Unloaded(object sender, RoutedEventArgs e)
  {
    // 에디터 내용을 저장 후 정리
    EditorViewModel.UpdateEditorBodyText();
    NoteEditorViewModelProvider.Release(ViewModel.Note);

    ImageCollectionViewModel.ImageViewModels.CollectionChanged -= ImageViewModels_CollectionChanged;

    // 노트 완전 삭제 로직
    bool deleteNote = SettingsService.Load(AppSettingsDescriptors.DeleteEmptyNote) && string.IsNullOrEmpty(ViewModel.Note.Title) && string.IsNullOrWhiteSpace(ViewModel.Note.BodyPlainText);
    if (deleteNote)
    {
      await ViewModel.DeleteNoteEntity();
    }

    NoteViewModelProvider.Release(ViewModel.Note);

    UnregisterMessengers();
    Bindings.StopTracking();
  }

  private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
  {
    sender.Changed -= AppWindow_Changed;
    sender.Closing -= AppWindow_Closing;

    if (_isManualClose)
    {
      ViewModel.Note.IsWindowOpen = false;
    }

    IntPtr hWnd = Win32Interop.GetWindowFromWindowId(sender.Id);
    if (hWnd != IntPtr.Zero)
    {
      // 원래 WndProc으로 복귀
      _ = NativeMethods.SetWindowLongPtr(hWnd, GWLP_WNDPROC, _oldWndProc);
    }
    _newWndProcCallback = null;
  }
  #endregion
}

partial class NotePage
{
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
        VisualStateManager.GoToState(this, nameof(FlyoutThemeDefault), false);
        break;
      case ElementTheme.Light:
        VisualStateManager.GoToState(this, nameof(FlyoutThemeLight), false);
        break;
      case ElementTheme.Dark:
        VisualStateManager.GoToState(this, nameof(FlyoutThemeDark), false);
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
    ViewModel.ImagePanelMaxHeight = Math.Min(this.ActualHeight * 0.5, 512 * this.XamlRoot.RasterizationScale);
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

  private void NotePage_ViewModeRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      switch (item.Tag)
      {
        case string tag when tag is "Edit":
          VisualStateManager.GoToState(this, nameof(ViewModeEdit), false);
          break;
        case string tag when tag is "ReadOnly":
          VisualStateManager.GoToState(this, nameof(ViewModeReadOnly), false);
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
        {
          suggestedFileName = $"MyNote_{DateTime.UtcNow:yyyyMMdd_hhmmss}";
        }

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

  private async void NotePage_BrowseButton_Click(object sender, RoutedEventArgs e)
  {
    if (WindowService.TryGetNoteWindowInfo(this, ViewModel.Note.Id, out _, out var appWindow))
    {
      FileOpenPicker picker = new(appWindow.Id);
      var result = await picker.PickSingleFileAsync();
      if (result is not null)
      {
        ViewModel.Note.BackgroundImagePath = result.Path;
      }
    }
  }

  private readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);
  private SolidColorBrush GetBackgroundBrush(BackdropKind backdropKind, Color color) => backdropKind is BackdropKind.None ? new(color) : _transparentBrush;

  private Visibility VisibleWhenAll(bool v1, bool v2) => v1 && v2 ? Visibility.Visible : Visibility.Collapsed;

  private void NotePage_ImagesContentSizer_PointerPressed(object sender, PointerRoutedEventArgs e)
  {
    if (FocusManager.GetFocusedElement(XamlRoot) is FrameworkElement focusedElement
        && focusedElement == NotePage_TextEditorRichEditBox)
    {
      NotePage_ImagesGridView.Focus(FocusState.Programmatic);
    }
  }

  private void ImageViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateNoteImagePaths();

  private void UpdateNoteImagePaths()
  {
    if (ImageCollectionViewModel.ImageViewModels is null)
    {
      return;
    }

    ViewModel.Note.Images = [.. ImageCollectionViewModel.ImageViewModels.Select(vm => vm.ImageDescriptor)];
    ViewModel.IsImagePanelVisible = ImageCollectionViewModel.ImageViewModels.Count > 0;
  }
  private void NotePage_ShowImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is FrameworkElement element && element.DataContext is ImageViewModel imageViewModel)
    {
      ImageCollectionViewModel.ShowImageCommand?.Execute(imageViewModel);
    }
  }

  private void NotePage_DeleteImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is FrameworkElement element && element.DataContext is ImageViewModel imageViewModel)
    {
      ImageCollectionViewModel.DeleteImageCommand?.Execute(imageViewModel);
    }
  }

  private void NotePage_UserImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
  {
    if (sender is FrameworkElement element && element.DataContext is ImageViewModel imageViewModel)
    {
      ImageCollectionViewModel.ShowImageCommand?.Execute(imageViewModel);
    }
  }
}

partial class NotePage
{
  #region 상단 타이틀 바 영역
  // 타이틀바 드래그 영역 조정(로드 및 크기 변경 시)
  private void NotePage_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void NotePage_TitleBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
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
  #endregion

  #region 에디터 영역
  private async void NotePage_TextEditorRichEditBox_Paste(object sender, TextControlPasteEventArgs e)
  {
    e.Handled = true;
    var clipboardDataPackageView = Clipboard.GetContent();
    var availableFormats = clipboardDataPackageView.AvailableFormats;
    var selection = NotePage_TextEditorRichEditBox.Document.Selection;

    if (availableFormats.Contains(StandardDataFormats.Rtf))
    {
      string rtfText = await clipboardDataPackageView.GetRtfAsync();
      selection.SetText(TextSetOptions.FormatRtf, rtfText);
      int position = selection.GetIndex(TextRangeUnit.Character) + rtfText.Length - 1;
      selection.SetRange(position, position);
    }
    else if (availableFormats.Contains(StandardDataFormats.Text))
    {
      string plainText = await clipboardDataPackageView.GetTextAsync();
      selection.SetText(TextSetOptions.None, plainText);
      int position = selection.GetIndex(TextRangeUnit.Character) + plainText.Length - 1;
      selection.SetRange(position, position);
    }
  }
  #endregion

  private bool IsBackdropPropertySliderEnabled(BackdropKind backdropKind) => backdropKind is not BackdropKind.None;
}

#region Keyboard Accelerators
partial class NotePage
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
      {
        _infoBarDismissTimer.Tick += InfoBarDismissTimer_Tick_WhenAutoClosed;
      }

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

    await ViewModel.UpdateNoteEntity();
    NotePage_InfoBar.Title = "Saved";
    NotePage_InfoBar.ActionButton = null;
    NotePage_InfoBar.Severity = InfoBarSeverity.Success;
    OpenInfoBar(TimeSpan.FromSeconds(2));
  }

  private void NotePage_FindKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    if (NotePage_FindReplaceBox.IsOpen)
    {
      VisualStateManager.GoToState(this, nameof(EditorSearchNone), false);
    }
    else
    {
      VisualStateManager.GoToState(this, nameof(EditorSearching), false);
    }
  }

  private void NotePage_ReplaceKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    if (NotePage_FindReplaceBox.IsOpen)
    {
      VisualStateManager.GoToState(this, nameof(EditorSearchNone), false);
    }
    else
    {
      VisualStateManager.GoToState(this, nameof(EditorSearching), false);
    }
  }

  private void NotePage_RenameTitleKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
  {
    VisualStateManager.GoToState(this, nameof(TitleBarTitleRename), false);
    NotePage_TitleRenameTextBox.Focus(FocusState.Keyboard);
    NotePage_TitleRenameTextBox.LayoutUpdated += NotePage_TitleRenameTextBox_LayoutUpdated;
  }
}
#endregion

#region 메신저 및 커맨드
partial class NotePage
{
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<ElementTheme>, MessageToken>(this, AppMessageTokens.ChangeAppThemeToken, new((recipient, message) => ChangeFlyoutTheme(message.Value)));

    WeakReferenceMessenger.Default.Register<ValueChangedMessage<WindowPresenterState>, MessageToken<NoteId>>(this, AppMessageTokens.NoteWindowActivationChangedToken(ViewModel.Note.Id), new((recipient, message) =>
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