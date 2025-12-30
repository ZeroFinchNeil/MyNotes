using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Debugging;
using MyNotes.Helpers;
using MyNotes.Models.Notes;
using MyNotes.Resources;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Notes;

namespace MyNotes.Views.Windows;

internal sealed partial class NoteWindow : Window
{
  private readonly WindowService WindowService;

  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;
  private readonly NoteId NoteId;

  public NoteWindow(Note note)
  {
#if DEBUG
    ReferenceTracker.NoteWindowReference.Add(this, AppWindow.Id.Value);
#endif
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;

    var provider = App.Instance.Services.GetRequiredService<NoteViewModelProvider>();
    WindowService = App.Instance.Services.GetRequiredService<WindowService>();
    
    NoteId = note.Id;
    WindowService.NoteWindows.Add(NoteId, new WeakReference<NoteWindow>(this));

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
    this.Closed += NoteWindow_Closed;

    this.Content = new NotePage(this, note);
  }

  private void NoteWindow_Closed(object sender, WindowEventArgs args)
  {
    Console.WriteLine("{0}: {1}", "NoteWindow_Closed", "");

    this.Activated -= NoteWindow_Activated;
    this.Closed -= NoteWindow_Closed;

    // WindowService에서 Window 테이블에서 제거
    WindowService.NoteWindows.Remove(NoteId);
  }

  private void NoteWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<WindowActivationState>(args.WindowActivationState), MessageTokens.NoteWindowActivationChangedToken(NoteId));
  }
}