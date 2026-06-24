using CommunityToolkit.Mvvm.ComponentModel;
using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Settings;
using D2RLootBeeper.Domain.Loot;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace D2RLootBeeper.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
  private readonly IItemBaseCatalog _catalog;
  private readonly ISettingsStore _settingsStore;
  private readonly IGameProcessService _gameProcessService;
  private readonly IKeyboardMonitor _keyboardMonitor;

  [ObservableProperty]
  private bool _isGameRunning;

  public ObservableCollection<CategoryViewModel> Categories { get; }

  public MainViewModel(
    IItemBaseCatalog catalog,
    ISettingsStore settingsStore,
    IGameProcessService gameProcessService,
    IKeyboardMonitor keyboardMonitor
  )
  {
    _catalog = catalog;
    _settingsStore = settingsStore;
    _gameProcessService = gameProcessService;
    _keyboardMonitor = keyboardMonitor;

    Categories = [];

    _keyboardMonitor.Start();
    Load();

    _keyboardMonitor.AltPressed += (_, _) => { Debug.WriteLine("ALT DETECTED"); };
  }

  private void Load()
  {
    IsGameRunning = _gameProcessService.IsRunning();

    UserSettings settings = _settingsStore.Load();

    HashSet<string> selected
      = new(settings.SelectedItemBases, StringComparer.OrdinalIgnoreCase);
    IOrderedEnumerable<IGrouping<ItemCategory, ItemBase>> grouped
      = _catalog.GetAll()
          .GroupBy(x => x.Category)
          .OrderBy(x => x.Key.ToString());

    foreach (IGrouping<ItemCategory, ItemBase> category in grouped)
    {
      IEnumerable<ItemBaseViewModel> items
        = category.OrderBy(x => x.Name)
            .Select(item => new ItemBaseViewModel(
              item.Name,
              selected.Contains(item.Name)
            ));

      Categories.Add(new(category.Key.ToString(), items));
    }
  }

  public void Save()
  {
    List<string> selected = [
      .. Categories.SelectMany(x => x.Items)
        .Where(x => x.IsSelected)
        .Select(x => x.Name)
    ];

    _settingsStore.Save(new() { SelectedItemBases = selected });
  }
}
