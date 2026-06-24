namespace D2RLootBeeper.Application.Contracts;

/// <summary>
/// Captures the current D2R game frame.
/// </summary>
public interface IGameCaptureService
{
  /// <summary>
  /// Captures a screenshot of the game.
  /// </summary>
  Task<byte[]> CaptureAsync(CancellationToken cToken);
}
