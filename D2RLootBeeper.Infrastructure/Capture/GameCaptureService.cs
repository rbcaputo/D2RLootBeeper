using D2RLootBeeper.Application.Contracts;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace D2RLootBeeper.Infrastructure.Capture;

/// <summary>
/// Captures the D2R game window using <c>PrintWindow(PW_RENDERFULLCONTENT)</c>,
/// which asks the DWM compositor to blit the DirectX swap-chain into a GDI DC.
/// 
/// Returns the frame as PNG-encoded bytes so the OCR service receives a
/// lossless, self-contained image with no temp-file I/O.
/// </summary>
public sealed class GameCaptureService : IGameCaptureService
{
  private static readonly string[] ProcessNames
    = ["D2R", "Diablo II Resurrected"];

  public async Task<byte[]> CaptureAsync(CancellationToken cToken)
  {
    cToken.ThrowIfCancellationRequested();

    IntPtr hwnd = FindD2RWindow();
    if (hwnd == IntPtr.Zero)
      return [];

    if (!Win32Methods.GetWindowRect(hwnd, out Win32Methods.RECT rect))
      return [];

    int width = rect.Right - rect.Left;
    int height = rect.Bottom - rect.Top;
    if (width <= 0 || height <= 0)
      return [];

    using Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);
    using (Graphics graphics = Graphics.FromImage(bitmap))
    {
      IntPtr hdc = graphics.GetHdc();

      try
      {
        Win32Methods.PrintWindow(hwnd, hdc, Win32Methods.PW_RENDERFULLCONTENT);
      }
      finally
      {
        graphics.ReleaseHdc();
      }
    }

    using MemoryStream stream = new();
    bitmap.Save(stream, ImageFormat.Png);

    // Return synchronously - bitmap encoding is CPU-bound and very fast.
    return await Task.FromResult(stream.ToArray());
  }

  private static IntPtr FindD2RWindow()
  {
    foreach (string name in ProcessNames)
    {
      Process[] matches = Process.GetProcessesByName(name);
      if (matches.Length > 0 && matches[0].MainWindowHandle != IntPtr.Zero)
        return matches[0].MainWindowHandle;
    }

    return IntPtr.Zero;
  }
}
