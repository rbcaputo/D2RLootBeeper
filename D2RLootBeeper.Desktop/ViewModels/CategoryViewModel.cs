using System.Collections.ObjectModel;

namespace D2RLootBeeper.Desktop.ViewModels;

/// <summary>
/// Represents a category group in the UI.
/// </summary>
public sealed class CategoryViewModel(string name, IEnumerable<ItemBaseViewModel> items)
{
  public string Name { get; } = name;

  public ObservableCollection<ItemBaseViewModel> Items { get; }
    = new(items);
}
