namespace D2RLootBeeper.Application.Settings;

/// <summary>
/// Represents persisted user preferences.
/// 
/// Stored in settings.json by the Infrastructure layer.
/// </summary>
public sealed class UserSettings
{
  /// <summary>
  /// Item base names selected by the user. (e.g. "Monarch, "Ber Rune").
  /// </summary>
  public IReadOnlyCollection<string> SelectedItemBases { get; init; } = [];

  /// <summary>
  /// Minimum fuzzy-match similarity score.
  /// Range: 0.0-1.0. Default: 0.85 (85% similarity).
  /// </summary>
  public double FuzzyMatchThreshold { get; init; } = 0.85;

  /// <summary>
  /// Frequency in Hz for the alert beep.
  /// </summary>
  public int BeepFrequencyHz { get; init; } = 900;

  /// <summary>
  /// Duration in milliseconds for the alert beep.
  /// Valid range: 1-5000. Default: 200ms.
  /// </summary>
  public int BeepDurationMs { get; init; } = 200;
}
