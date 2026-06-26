using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Settings;
using D2RLootBeeper.Domain.Loot;
using System.Collections.ObjectModel;

namespace D2RLootBeeper.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
  private readonly IItemBaseCatalog _catalog;
  private readonly ISettingsStore _settingsStore;
  private readonly IGameProcessService _gameProcessService;

  [ObservableProperty]
  private bool _isGameRunning;

  public ObservableCollection<CategoryViewModel> Categories { get; } = [];

  public MainViewModel(
    IItemBaseCatalog catalog,
    ISettingsStore settingsStore,
    IGameProcessService gameProcessService
  )
  {
    _catalog = catalog;
    _settingsStore = settingsStore;
    _gameProcessService = gameProcessService;

    Load();
  }

  // --- Commands -----

  [RelayCommand]
  private void Save()
  {
    IEnumerable<string> selected = Categories
      .SelectMany(c => c.Items)
      .Where(i => i.IsSelected)
      .Select(i => i.Name);

    UserSettings current = _settingsStore.Load();
    UserSettings updated = current with
    {
      SelectedItemBases = [.. selected]
    };

    _settingsStore.Save(updated);
  }

  private void Load()
  {
    IsGameRunning = _gameProcessService.IsRunning();

    UserSettings settings = _settingsStore.Load();

    HashSet<string> selected
      = new(settings.SelectedItemBases, StringComparer.OrdinalIgnoreCase);

    // Group by DisplayGroup (e.g. "Axe", "Sword", "Circlet", "Rune", "Key").
    // Sort by domain category first so related slots appear together,
    // then alphabetically within each actegory group.
    var groups = _catalog.GetAll()
      .GroupBy(x => x.DisplayGroup)
      .OrderBy(g => CategoryOrder(g.First().Category))
      .ThenBy(g => g.Key);

    foreach (var group in groups)
    {
      IEnumerable<ItemBaseViewModel> items = group
        .OrderBy(x => x.Name)
        .Select(x => new ItemBaseViewModel(x.Name, selected.Contains(x.Name)));

      Categories.Add(new(group.Key, items));
    }
  }

  /// <summary>
  /// Controls the order in which category groups appear in the UI.
  /// 
  /// Runes lead (most commonly watched), then weapons by sub-type,
  /// then armor slots top-to-bottom, then jewelry and collectibles.
  /// </summary>
  private static int CategoryOrder(ItemCategory category)
    => category switch
    {
      ItemCategory.Rune => 0,
      ItemCategory.Weapon => 1,
      ItemCategory.Helmet => 2,
      ItemCategory.Torso => 3,
      ItemCategory.Shield => 4,
      ItemCategory.Belt => 5,
      ItemCategory.Boots => 6,
      ItemCategory.Gloves => 7,
      ItemCategory.Ring => 8,
      ItemCategory.Amulet => 9,
      ItemCategory.Charm => 10,
      ItemCategory.Jewel => 11,
      ItemCategory.Gem => 12,
      ItemCategory.Material => 13,
      _ => 99
    };
}
