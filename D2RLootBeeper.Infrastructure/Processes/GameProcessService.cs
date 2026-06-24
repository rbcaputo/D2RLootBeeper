using D2RLootBeeper.Application.Contracts;
using System.Diagnostics;

namespace D2RLootBeeper.Infrastructure.Processes;

/// <summary>
/// Detects whether D2R is currently running.
/// </summary>
public sealed class GameProcessService : IGameProcessService
{
  /// <summary>
  /// Known D2R process names.
  /// 
  /// We support multiple names because Blizzard occasionally changes launchers.
  /// </summary>
  private static readonly string[] ProcessName
    = ["D2R", "Diablo II Resurrected"];

  public bool IsRunning()
  {
    foreach (string processName in ProcessName)
      if (Process.GetProcessesByName(processName).Length > 0)
        return true;

    return false;
  }
}
