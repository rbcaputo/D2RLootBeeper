using D2RLootBeeper.Application.Settings;

namespace D2RLootBeeper.Application.Contracts;

/// <summary>
/// Loads and saves user settings.
/// </summary>
public interface ISettingsStore
{
  UserSettings Load();

  void Save(UserSettings settings);
}
