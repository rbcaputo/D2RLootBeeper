using CommunityToolkit.Mvvm.ComponentModel;

namespace D2RLootBeeper.Desktop.ViewModels;

/// <summary>
/// Represents a selectable item base in the UI.
/// </summary>
public partial class ItemBaseViewModel(string name, bool isSelected) : ObservableObject
{
  public string Name { get; } = name;

  [ObservableProperty]
  private bool _isSelected = isSelected;
}
