using D2RLootBeeper.Application.Contracts;

namespace D2RLootBeeper.Application.Monitoring;

/// <summary>
/// Coordinates the monitoring workflow.
/// 
/// Final workflow:
/// ALT Pressed > Capture > OCR > Match > Alert
/// </summary>
public sealed class LootMonitoringService
{
  private readonly IKeyboardMonitor _keyboardMonitor;

  public LootMonitoringService(IKeyboardMonitor keyboardMonitor)
  {
    _keyboardMonitor = keyboardMonitor;

    _keyboardMonitor.AltPressed += OnAltPressed;
  }

  private void OnAltPressed(object? sender, EventArgs ea)
  {
    // TODO: Capture frame > Run OCR > Match items > Trigger alert
  }
}
