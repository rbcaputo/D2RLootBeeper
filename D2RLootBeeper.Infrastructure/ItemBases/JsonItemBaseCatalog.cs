using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Domain.Loot;
using System.Text.Json;

namespace D2RLootBeeper.Infrastructure.ItemBases;

/// <summary>
/// Loads item bases from item-bases.json.
/// </summary>
public sealed class JsonItemBaseCatalog : IItemBaseCatalog
{
  private const string FileName = "Data/item-bases.json";
  private readonly IReadOnlyCollection<ItemBase> _itemBases;

  public JsonItemBaseCatalog()
    => _itemBases = Load();

  public IReadOnlyCollection<ItemBase> GetAll()
    => _itemBases;

  private static List<ItemBase> Load()
  {
    if (!File.Exists(FileName))
      throw new FileNotFoundException($"Item base file not found: {FileName}");

    string json = File.ReadAllText(FileName);

    return JsonSerializer.Deserialize<List<ItemBase>>(json)
      ?? [];
  }
}
