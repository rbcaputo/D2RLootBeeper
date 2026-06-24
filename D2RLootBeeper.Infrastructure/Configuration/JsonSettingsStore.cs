using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Settings;
using System.Text.Json;

namespace D2RLootBeeper.Infrastructure.Configuration;

/// <summary>
/// Persists user settings to disk.
/// </summary>
public sealed class JsonSettingsStore : ISettingsStore
{
  private const string FileName = "settings.json";
  private static readonly JsonSerializerOptions SerializerOptions
    = new() { WriteIndented = true };

  public UserSettings Load()
  {
    if (!File.Exists(FileName))
      return new();

    string json = File.ReadAllText(FileName);

    return JsonSerializer.Deserialize<UserSettings>(json, SerializerOptions)
      ?? new();
  }

  public void Save(UserSettings settings)
  {
    ArgumentNullException.ThrowIfNull(settings);

    string json
      = JsonSerializer.Serialize(settings, SerializerOptions);

    File.WriteAllText(FileName, json);
  }
}
