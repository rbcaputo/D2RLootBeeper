namespace D2RLootBeeper.Domain.Loot;

/// <summary>
/// Represents a D2R item base.
/// Examples:
/// - Monarch
/// - Berserker Axe
/// - Chain Gloves
/// </summary>
public sealed record ItemBase(string Name, ItemCategory Category);
