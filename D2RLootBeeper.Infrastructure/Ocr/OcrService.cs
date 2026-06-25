using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Domain.Loot;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Tesseract;

namespace D2RLootBeeper.Infrastructure.Ocr;

/// <summary>
/// Runs Tesseract OCR over a captured game frame and returns all
/// detected text tokes as <see cref="DetectionResult"/> values.
/// 
/// <para>
/// <strong>Preprocessing:</strong>
/// the raw PNG frame is upscaled 2x with nearest-neighbor interpolation before being handed to Tesseract.
/// This gives the LSTM engine larger, crisper letter forms without introducing the blurring that bicubic would cause.
/// </para>
/// 
/// <para>
/// <strong>PSM:</strong>
/// <c>SparseText</c> (11) is used because item labels are disconnected fragments scattered across the viewport -
/// there is no reading order or paragraph structure.
/// </para>
/// 
/// <para>
/// <strong>Thread safety:</strong>
/// <see cref="TesseractEngine"/> is not thread-safe; a <see cref="SemaphoreSlim"/> serializes concurrent callers.
/// In practice only one ALT-triggered pass runs at a time, so there is no throughput cost.
/// </para>
/// </summary>
public sealed class OcrService : IOcrService, IDisposable
{
  /// <summary>
  /// Directory that contains <c>eng.traineddata</c>.
  /// Place the file from <see href="https://github.com/tesseract-ocr/tessdata_fast">tessdata_fast</see>
  /// next to the executable in a folder named <c>tessdata</c>.
  /// </summary>
  private const string TessDataPath = "tessdata";

  /// <summary>
  /// Scale factor applier before OCR. 2x is a good speed/accuracy trade-off.
  /// </summary>
  private const int UpscaleFactor = 2;

  /// <summary>
  /// Minimum per-word Tesseract confidence (0-100) to include in results.
  /// </summary>
  private const float MinWordConfidence = 30f;

  private readonly TesseractEngine _engine;
  private readonly SemaphoreSlim _lock = new(1, 1);

  public OcrService()
  {
    _engine = new(TessDataPath, "eng", EngineMode.LstmOnly);

    // Restrict charset to what can appear in D2R item base names.
    _engine.SetVariable(
      "tessedit_char_whitelist",
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz '-"
    );
  }

  public async Task<IReadOnlyCollection<DetectionResult>> DetectAsync(
    byte[] imageData,
    CancellationToken cToken
  )
  {
    if (imageData.Length == 0)
      return [];

    cToken.ThrowIfCancellationRequested();

    await _lock.WaitAsync(cToken);

    try
    {
      return RunOcr(imageData);
    }
    finally
    {
      _lock.Release();
    }
  }

  // --- Private helpers -----

  private List<DetectionResult> RunOcr(byte[] imageData)
  {
    using MemoryStream stream = new(imageData);
    using Bitmap source = new(stream);
    using Bitmap processed = Upscale(source);

    List<DetectionResult> results = [];

    using var pix = ToBitmapPix(processed);
    using Page page = _engine.Process(pix, PageSegMode.SparseText);
    using ResultIterator iterator = page.GetIterator();

    iterator.Begin();

    do
    {
      float confidence = iterator.GetConfidence(PageIteratorLevel.Word);
      if (confidence < MinWordConfidence)
        continue;

      string? raw = iterator.GetText(PageIteratorLevel.Word)?.Trim();
      if (string.IsNullOrWhiteSpace(raw) || raw.Length < 2)
        continue;

      string normalized = raw.ToLowerInvariant();
      results.Add(new(raw, normalized, confidence / 100f));
    }
    while (iterator.Next(PageIteratorLevel.Word));

    return results;
  }

  private static Bitmap Upscale(Bitmap source)
  {
    int width = source.Width * UpscaleFactor;
    int height = source.Height * UpscaleFactor;

    Bitmap result = new(width, height, PixelFormat.Format32bppArgb);

    using Graphics graphics = Graphics.FromImage(result);

    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
    graphics.PixelOffsetMode = PixelOffsetMode.Half;

    graphics.DrawImage(source, x: 0, y: 0, width, height);

    return result;
  }

  /// <summary>
  /// Converts a <see cref="Bitmap"/> to a Tesseract <see cref="Pix"/> via an
  /// in-memory PNG stream - the most reliable cross-version path.
  /// </summary>
  private static Pix ToBitmapPix(Bitmap bitmap)
  {
    using MemoryStream stream = new();

    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

    return Pix.LoadFromMemory(stream.ToArray());
  }

  public void Dispose()
  {
    _engine.Dispose();
    _lock.Dispose();
  }
}
