using MyNotes.Common.Interop;
using MyNotes.Constants;
using MyNotes.Widget;

namespace MyNotes;

public class Program
{
  [STAThread]
  private static int Main(string[] args)
  {
    WinRT.ComWrappersSupport.InitializeComWrappers();
    Debug.WriteLine("{0}: {1}", "Main args", string.Join(", ", args));

    LaunchAppSingleInstance();

    return 0;
  }

  public static WidgetProvider? WidgetProvider { get; private set; }

  private static bool IsOSVersionAtLeast(int major, int minor, int build, int revision = 0)
  {
    ulong version = ulong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
    var curMajor = (int)((version & 0xFFFF000000000000L) >> 48);
    var curMinor = (int)((version & 0x0000FFFF00000000L) >> 32);
    var curBuild = (int)((version & 0x00000000FFFF0000L) >> 16);
    var curRevision = (int)(version & 0x000000000000FFFFL);

    if (curMajor != major)
      return curMajor > major;

    if (curMinor != minor)
      return curMinor > minor;

    if (curBuild != build)
      return curBuild >= build;

    return curRevision >= revision;
  }

  public static bool IsWindowsVersion11OrHigher => IsOSVersionAtLeast(10, 0, 22000);

  private static void LaunchAppSingleInstance()
  {
    bool shouldCreatePrimaryAppInstance = !DecideRedirection();
    if (shouldCreatePrimaryAppInstance)
    {
#if DEBUG
      IntPtr consoleHWND = IntPtr.Zero;
      //if (Debugger.IsAttached)
      consoleHWND = NativeMethods.SetConsole(0, 300, 800, 1000);
#endif
      App? app = null;
      Application.Start((p) =>
      {
        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
        app = new App();
      });
      app?.Dispose();

#if DEBUG
      //if (Debugger.IsAttached)
      //{
      NativeMethods.FreeConsole();
      NativeMethods.SendMessage(consoleHWND, (uint)NativeMethods.WindowMessage.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
      //}
#endif
    }
  }

  private static bool DecideRedirection()
  {
    AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
    AppInstance keyInstance = AppInstance.FindOrRegisterForKey("MyNotes");

    // AppInstance.IsCurrent는 AppInstance가 현재 앱의 인스턴스인지 아니면 다른 인스턴스인지를 반환합니다.
    // true이면 기존에 실행 중인 앱 인스턴스가 없다는 것을 의미하고(초기 앱 실행),
    // false이면 기존에 실행 중인 앱 인스턴스가 있다는 것을 의미합니다.
    if (keyInstance.IsCurrent)
    {
      keyInstance.Activated += OnActivated;
    }
    else
    {
      RedirectActivationTo(args, keyInstance);
      return true;
    }

    return false;
  }

  private static void OnActivated(object? sender, AppActivationArguments args)
  {
    ExtendedActivationKind kind = args.Kind;

    // Widget
    if (IsWindowsVersion11OrHigher)
    {
      Guid CLSID_Factory = Guid.Parse(AppStrings.WidgetProvider_COM_CLSID);
      WidgetProviderFactory<WidgetProvider> widgetProviderFactory = new();
      WidgetProvider = widgetProviderFactory.Instance;
      _ = NativeMethods.CoRegisterClassObject(CLSID_Factory, widgetProviderFactory, 0x4, 0x1, out uint cookie);

      if (kind is ExtendedActivationKind.StartupTask)
      {
        using (var emptyWidgetListEvent = WidgetProvider.EmptyWidgetListEvent)
        {
          emptyWidgetListEvent.WaitOne();
          WidgetProvider = null;
          _ = NativeMethods.CoRevokeClassObject(cookie);
        }
      }
    }
  }

  private static IntPtr _redirectEventHandle = IntPtr.Zero;

  /// <summary>
  /// <para>Redirects the current app activation to the specified key instance, bringing its main window to the foreground.</para>
  /// <para>현재 앱 활성화를 지정된 키 인스턴스로 리디렉션하여 해당 앱의 메인 창을 포그라운드로 가져옵니다.</para>
  /// </summary>
  /// <remarks>
  /// <para>Do the redirection on another thread, and use a non-blocking wait method to wait for the redirection to complete.</para>
  /// <para>리디렉션은 다른 스레드에서 수행하고, 리디렉션이 완료될 때까지 논블로킹 대기 방식을 사용하세요.</para>
  /// </remarks>
  private static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
  {
    _redirectEventHandle = NativeMethods.CreateEvent(IntPtr.Zero, true, false, null);
    Task.Run(() =>
    {
      keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
      NativeMethods.SetEvent(_redirectEventHandle);
    });

    uint CWMO_DEFAULT = 0;
    uint INFINITE = 0xFFFFFFFF;
    _ = NativeMethods.CoWaitForMultipleObjects(CWMO_DEFAULT, INFINITE, 1, [_redirectEventHandle], out uint handleIndex);

    // Bring the window to the foreground
    Process process = Process.GetProcessById((int)keyInstance.ProcessId);

    int SW_RESTORE = 9;
    NativeMethods.ShowWindowAsync(process.MainWindowHandle, SW_RESTORE);

    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
  }
}

