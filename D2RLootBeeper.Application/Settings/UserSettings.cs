namespace D2RLootBeeper.Application.Settings;

/// <summary>
/// Represents persisted user preferences.
/// 
/// This object is stored in settings.json by the Infrastructure layer.
/// </summary>
public sealed class UserSettings
{
  /// <summary>
  /// Item bases selected by the user.
  /// </summary>
  public IReadOnlyCollection<string> SelectedItemBases { get; init; } = [];

  /// <summary>
  /// Minimum fuzzy-match similarity score.
  /// 
  /// Example:
  /// 0.85 = 85% similarity required.
  /// </summary>
  public double FuzzyMatchThreshold { get; init; } = 0.85;
}
