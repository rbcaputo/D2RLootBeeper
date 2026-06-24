using System.Runtime.InteropServices;

namespace D2RLootBeeper.Infrastructure.Input;

internal static class NativeMethods
{
  internal const int WH_KEYBOARD_LL = 13;
  internal const int WM_KEYDOWN = 0x0100;
  internal const int VK_LMENU = 0xA4;

  internal delegate IntPtr HookProc(
    int nCode,
    IntPtr wParam,
    IntPtr lParam
  );

  [DllImport("user32.dll")]
  internal static extern IntPtr SetWindowsHookEx(
    int idHook,
    HookProc lpfn,
    IntPtr hMod,
    uint dwThreadId
  );

  [DllImport("user32.dll")]
  internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

  [DllImport("user32.dll")]
  internal static extern IntPtr CallNextHookEx(
    IntPtr hhk,
    int nCode,
    IntPtr wParam,
    IntPtr lParam
  );

  [DllImport("kernel32.dll")]
  internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string? lpModuleName);
}
