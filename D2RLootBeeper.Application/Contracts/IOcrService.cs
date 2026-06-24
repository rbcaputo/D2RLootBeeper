using D2RLootBeeper.Domain.Loot;

namespace D2RLootBeeper.Application.Contracts;

/// <summary>
/// Performs OCR against a captured frame.
/// </summary>
public interface IOcrService
{
  Task<IReadOnlyCollection<DetectionResult>> DetectAsync(byte[] imageData, CancellationToken cToken);
}
