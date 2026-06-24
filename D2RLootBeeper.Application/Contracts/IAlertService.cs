namespace D2RLootBeeper.Application.Contracts;

/// <summary>
/// Notifies the user when a watched item has been detected.
/// </summary>
public interface IAlertService
{
  Task AlertAsync(CancellationToken cToken);
}
