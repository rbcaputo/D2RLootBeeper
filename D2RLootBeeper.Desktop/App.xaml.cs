using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Monitoring;
using D2RLootBeeper.Desktop.ViewModels;
using D2RLootBeeper.Desktop.Views;
using D2RLootBeeper.Infrastructure.Alert;
using D2RLootBeeper.Infrastructure.Capture;
using D2RLootBeeper.Infrastructure.Configuration;
using D2RLootBeeper.Infrastructure.FuzzyMatcher;
using D2RLootBeeper.Infrastructure.Input;
using D2RLootBeeper.Infrastructure.ItemBases;
using D2RLootBeeper.Infrastructure.Ocr;
using D2RLootBeeper.Infrastructure.Processes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace D2RLootBeeper.Desktop;

public partial class App : System.Windows.Application
{
  private readonly IHost _host;

  public App()
  {
    _host = Host
      .CreateDefaultBuilder()
      .ConfigureServices(ConfigureServices)
      .Build();
  }

  private static void ConfigureServices(IServiceCollection services)
  {
    // --- Infrastructure -----
    services.AddSingleton<IGameProcessService, GameProcessService>();
    services.AddSingleton<ISettingsStore, JsonSettingsStore>();
    services.AddSingleton<IItemBaseCatalog, JsonItemBaseCatalog>();
    services.AddSingleton<IKeyboardMonitor, GlobalKeyboardMonitor>();
    services.AddSingleton<IGameCaptureService, GameCaptureService>();
    services.AddSingleton<IOcrService, OcrService>();
    services.AddSingleton<IAlertService, AlertService>();
    services.AddSingleton<IFuzzyMatcher, FuzzyMatcher>();

    // --- Application ------
    services.AddSingleton<LootMonitoringService>();

    // --- Presentation -----
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<MainWindow>();
  }

  protected override async void OnStartup(StartupEventArgs ea)
  {
    await _host.StartAsync();

    // Start the keyboard hook and loot detection pipeline.
    _host.Services.GetRequiredService<LootMonitoringService>().Start();

    MainWindow window
      = _host.Services.GetRequiredService<MainWindow>();

    window.Show();
    base.OnStartup(ea);
  }

  protected override async void OnExit(ExitEventArgs ea)
  {
    _host.Services.GetRequiredService<LootMonitoringService>().Stop();

    await _host.StopAsync();

    _host.Dispose();
    base.OnExit(ea);
  }
}
