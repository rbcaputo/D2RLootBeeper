using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Domain.Loot;
using System.Diagnostics;
using System.Text.Json;

namespace D2RLootBeeper.Infrastructure.ItemBases;

/// <summary>
/// Loads the item base catalog from <c>Data/item-bases.json</c> and maps each entry to a domain <see cref="ItemBase"/>.
/// 
/// <para>JSON schema:</para>
/// <code>
/// {
///   "Supertype": "Weapon" | "Armor" | "Misc",
///   "Type": "One-Handed" | "Two-Handed" | "Belt" | "Torso" | ... | "Rune" | "Gem" | ...,
///   "Subtype": "Axe" | "Sword" | "Circlet" | "Pelt" | "Key" | ... (optional),
///   "Base": "Phase Blade" ← the floor-label text matched by OCR
/// }
/// </code>
/// 
/// <para>
/// <strong>DisplayGroup</strong> is derived as <c>Souvtype ?? Type</c>,
/// giving each entry sub-classification (e.g. "Axe", "Circlet", "Rune", "Key") that the
/// UI uses for grouping instead of the coarser <see cref="ItemCategory"/>.
/// </para>
/// </summary>
public sealed class JsonItemBaseCatalog : IItemBaseCatalog
{
  private const string FileName = "Data/item-bases.json";
  private static readonly JsonSerializerOptions JsonOptions
    = new() { WriteIndented = true };
  private readonly IReadOnlyCollection<ItemBase> _items;

  public JsonItemBaseCatalog()
    => _items = Load();

  public IReadOnlyCollection<ItemBase> GetAll()
    => _items;

  private static List<ItemBase> Load()
  {
    if (!File.Exists(FileName))
      throw new FileNotFoundException(
        $"Item base file not found: '{FileName}'. " +
        $"Ensure the file is set to CopyToOuputDirectory in the project."
      );

    string json = File.ReadAllText(FileName);
    List<ItemBaseDto>? dtos
      = JsonSerializer.Deserialize<List<ItemBaseDto>>(json, JsonOptions);

    return dtos?.Select(Map).ToList() ?? [];
  }

  private static ItemBase Map(ItemBaseDto dto)
  {
    ItemCategory category = ResolveCategory(dto);
    string displayGroup = dto.Subtype ?? dto.Type;

    return new(dto.Base, category, displayGroup);
  }

  /// <summary>
  /// Maps Supertype + Type to the domain <see cref="ItemCategory"/>.
  /// Extend this switch as new Type values are added to the item-base.json.
  /// </summary>
  private static ItemCategory ResolveCategory(ItemBaseDto dto)
    => (dto.Supertype, dto.Type) switch
    {
      ("Weapon", _) => ItemCategory.Weapon,

      ("Armor", "Torso") => ItemCategory.Torso,
      ("Armor", "Helmet") => ItemCategory.Helmet,
      ("Armor", "Belt") => ItemCategory.Belt,
      ("Armor", "Boots") => ItemCategory.Boots,
      ("Armor", "Gloves") => ItemCategory.Gloves,
      ("Armor", "Shield") => ItemCategory.Shield,

      (_, "Rune") => ItemCategory.Rune,
      (_, "Ring") => ItemCategory.Ring,
      (_, "Amulet") => ItemCategory.Amulet,
      (_, "Charm") => ItemCategory.Charm,
      (_, "Jewel") => ItemCategory.Jewel,
      (_, "Gem") => ItemCategory.Gem,
      (_, "Material") => ItemCategory.Material,

      _ => Fallback(dto)
    };

  private static ItemCategory Fallback(ItemBaseDto dto)
  {
    Debug.WriteLine(
      $"[JsonItemBaseCatalog] Unknown type mapping: Supertype='{dto.Supertype}' Type='{dto.Type}'. " +
      "Defaulting to Weapon. Update ResolveCategory if this is intentional."
    );

    return ItemCategory.Weapon;
  }
}
