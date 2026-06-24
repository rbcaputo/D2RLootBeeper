using D2RLootBeeper.Application.Contracts;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace D2RLootBeeper.Infrastructure.Input
{
  /// <summary>
  /// Global low-level keyboard hook.
  /// 
  /// Detects Left ALT presses even when D2R is focused.
  /// </summary>
  public sealed class GlobalKeyboardMonitor : IKeyboardMonitor
  {
    private NativeMethods.HookProc? _hookProc;
    private IntPtr _hookHandle;

    public event EventHandler? AltPressed;

    public void Start()
    {
      if (_hookHandle != IntPtr.Zero)
        return;

      _hookProc = HookCallback;

      using Process process = Process.GetCurrentProcess();
      using ProcessModule? module = process.MainModule;

      nint moduleHandle = NativeMethods.GetModuleHandle(module?.ModuleName);

      _hookHandle = NativeMethods.SetWindowsHookEx(
        NativeMethods.WH_KEYBOARD_LL,
        _hookProc,
        moduleHandle,
        dwThreadId: 0
      );
    }

    public void Stop()
    {
      if (_hookHandle == IntPtr.Zero)
        return;

      NativeMethods.UnhookWindowsHookEx(_hookHandle);

      _hookHandle = IntPtr.Zero;
    }

    public void Dispose()
    {
      Stop();
      GC.SuppressFinalize(this);
    }

    private IntPtr HookCallback(
      int nCode,
      IntPtr wParam,
      IntPtr lParam
    )
    {
      if (nCode >= 0 && wParam == NativeMethods.WM_KEYDOWN)
      {
        int keyCode = Marshal.ReadInt32(lParam);
        if (keyCode == NativeMethods.VK_LMENU)
          AltPressed?.Invoke(this, EventArgs.Empty);
      }

      return NativeMethods.CallNextHookEx(
        _hookHandle,
        nCode,
        wParam,
        lParam
      );
    }
  }
}
