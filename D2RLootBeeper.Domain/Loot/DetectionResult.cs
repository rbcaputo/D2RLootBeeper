namespace D2RLootBeeper.Domain.Loot;

/// <summary>
/// Represents a successfully recognized item label.
/// </summary>
/// <param name="RawText">Original OCR output.</param>
/// <param name="NormalizedText">Cleaned text used for matching.</param>
/// <param name="Confidence">OCR confidence score between 0 and 1.</param>
public sealed record DetectionResult(string RawText, string NormalizedText, float Confidence);
