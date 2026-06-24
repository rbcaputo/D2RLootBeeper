using D2RLootBeeper.Domain.Loot;

namespace D2RLootBeeper.Application.Contracts;

/// <summary>
/// Provides access to the complete item base catalog.
/// </summary>
public interface IItemBaseCatalog
{
  IReadOnlyCollection<ItemBase> GetAll();
}
