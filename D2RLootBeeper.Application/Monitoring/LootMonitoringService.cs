using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Settings;
using D2RLootBeeper.Domain.Loot;

namespace D2RLootBeeper.Application.Monitoring;

/// <summary>
/// Coordinates the full loot detection workflow.
/// 
/// Pipeline: ALT pressed → capture → OCR → fuzzy match → alert.
/// 
/// Triggered exclusively by the global keyboard hook; no polling loop.
/// A re-entry guard ensures that a slow OCR pass does not stack with the next ALT press.
/// </summary>
public sealed class LootMonitoringService : IDisposable
{
  private readonly IKeyboardMonitor _keyboardMonitor;
  private readonly IGameProcessService _gameProcessService;
  private readonly IGameCaptureService _gameCaptureService;
  private readonly IOcrService _ocrService;
  private readonly IAlertService _alertService;
  private readonly ISettingsStore _settingsStore;
  private readonly IItemBaseCatalog _catalog;
  private readonly IFuzzyMatcher _fuzzyMatcher;

  /// <summary>
  /// Interlocked flag: true while a detection pass is in progress.
  /// Prevents concurrent passes when ALT is pressed repeatedly.
  /// </summary>
  private bool _isProcessing;

  /// <summary>
  /// Minimum OCR word confidence to forward to fuzzy matching.
  /// </summary>
  private const float MinOcrConfidence = 0.30f;

  public LootMonitoringService(
    IKeyboardMonitor keyboardMonitor,
    IGameProcessService gameProcessService,
    IGameCaptureService gameCaptureService,
    IOcrService ocrService,
    IAlertService alertService,
    ISettingsStore settingsStore,
    IItemBaseCatalog catalog,
    IFuzzyMatcher fuzzyMatcher
  )
  {
    _keyboardMonitor = keyboardMonitor;
    _gameProcessService = gameProcessService;
    _gameCaptureService = gameCaptureService;
    _ocrService = ocrService;
    _alertService = alertService;
    _settingsStore = settingsStore;
    _catalog = catalog;
    _fuzzyMatcher = fuzzyMatcher;

    _keyboardMonitor.AltPressed += OnAltPressed;
  }

  public void Start()
    => _keyboardMonitor.Start();

  public void Stop()
    => _keyboardMonitor.Stop();

  private async void OnAltPressed(object? sender, EventArgs ea)
  {
    if (Interlocked.CompareExchange(ref _isProcessing, true, false) != false)
      return;

    try
    {
      await RunPipelineAsync();
    }
    catch (Exception ex)
    {
      // async void cannot propagate - surface to debug output.
      System.Diagnostics.Debug.WriteLine($"[LootMonitoringService] {ex}");
    }
    finally
    {
      Interlocked.Exchange(ref _isProcessing, false);
    }
  }

  private async Task RunPipelineAsync()
  {
    if (!_gameProcessService.IsRunning())
      return;

    // Hard timeout: each pass must complete within 5 seconds.
    CancellationTokenSource cTokenSource = new(TimeSpan.FromSeconds(5));
    CancellationToken cToken = cTokenSource.Token;

    // 1. Capture
    byte[] frame = await _gameCaptureService.CaptureAsync(cToken);
    if (frame.Length == 0)
      return;

    // 2. OCR
    IReadOnlyCollection<DetectionResult> detections
      = await _ocrService.DetectAsync(frame, cToken);
    if (detections.Count == 0)
      return;

    // 3. Load current settings + build watch list
    //    Re-read on every pass so UI changes take affect immediately.
    UserSettings settings = _settingsStore.Load();
    WatchList watchList = BuildWatchList(settings);
    if (watchList.Items.Count == 0)
      return;

    // 4. Fuzzy match: any OCR token above confidence vs. any watched item
    bool matched = false;
    foreach (DetectionResult result in detections)
    {
      if (result.Confidence < MinOcrConfidence)
        continue;

      foreach (ItemBase item in watchList.Items)
      {
        double similarity
          = _fuzzyMatcher.Similarity(result.NormalizedText, item.Name);
        if (similarity >= settings.FuzzyMatchThreshold)
        {
          System.Diagnostics.Debug.WriteLine($"[Match] '{result.NormalizedText}' - '{item.Name}' ({similarity:P0})");

          matched = true;

          break;
        }
      }

      if (matched)
        break;
    }

    // 5. Alert
    if (matched)
      await _alertService.AlertAsync(cToken);
  }

  private WatchList BuildWatchList(UserSettings settings)
  {
    HashSet<string> selected
      = new(settings.SelectedItemBases, StringComparer.OrdinalIgnoreCase);
    IEnumerable<ItemBase> bases = _catalog.GetAll()
      .Where(b => selected.Contains(b.Name));

    return new(bases);
  }

  public void Dispose()
  {
    _keyboardMonitor.AltPressed -= OnAltPressed;
    _keyboardMonitor.Dispose();
  }
}
