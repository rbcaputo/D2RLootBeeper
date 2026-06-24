using D2RLootBeeper.Desktop.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace D2RLootBeeper.Desktop.Views;

public partial class MainWindow : Window
{
  private readonly MainViewModel _viewModel;

  public MainWindow(MainViewModel viewModel)
  {
    InitializeComponent();

    _viewModel = viewModel;
    DataContext = _viewModel;
  }

  protected override void OnClosing(CancelEventArgs ea)
  {
    _viewModel.Save();
    base.OnClosing(ea);
  }
}