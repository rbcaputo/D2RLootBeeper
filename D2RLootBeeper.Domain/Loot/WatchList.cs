namespace D2RLootBeeper.Domain.Loot;

/// <summary>
/// Represents the user's selected item bases.
/// 
/// Internally uses case-insensitive comparison bacause OCR normalization is case-insensitive.
/// </summary>
public sealed class WatchList
{
  private readonly HashSet<ItemBase> _items;

  public WatchList(IEnumerable<ItemBase> items)
  {
    ArgumentNullException.ThrowIfNull(items);

    _items = [.. items];
  }

  /// <summary>
  /// Returns all selected item bases.
  /// </summary>
  public IReadOnlyCollection<ItemBase> Items
  => _items;

  /// <summary>
  /// Determines whether the specified item base is part of the watch list.
  /// </summary>
  public bool Contains(ItemBase item)
  {
    ArgumentNullException.ThrowIfNull(item);

    return _items.Contains(item);
  }
}
