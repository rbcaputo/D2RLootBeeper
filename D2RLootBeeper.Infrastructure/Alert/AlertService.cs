using D2RLootBeeper.Application.Contracts;
using D2RLootBeeper.Application.Settings;

namespace D2RLootBeeper.Infrastructure.Alert;

/// <summary>
/// Emits an audible beep via <see cref="Console.Beep(int,int)"/> (Win32 <c>Beep</c>).
/// 
/// The beep is fired on a thread-pool thread so it never blocks the UI or the detection pipeline.
/// An interlocked guard prevents overlapping beeps when two matches arrive in quick sucession.
/// </summary>
public sealed class AlertService(ISettingsStore settingsStore) : IAlertService
{
  private readonly ISettingsStore _settingsStore = settingsStore;
  private bool _isBeeping; // interlocked: false = idle, true = playing

  public Task AlertAsync(CancellationToken cToken)
  {
    // Skip if a ceep is already in progress.
    if (Interlocked.CompareExchange(ref _isBeeping, true, false) != false)
      return Task.CompletedTask;

    UserSettings settings = _settingsStore.Load();

    int frequency = Math.Clamp(settings.BeepFrequencyHz, min: 37, max: 32_767);
    int duration = Math.Clamp(settings.BeepDurationMs, min: 1, max: 5_000);

    ThreadPool.QueueUserWorkItem(_ =>
    {
      try
      {
        Console.Beep(frequency, duration);
      }
      catch
      {
        // suppress - some headless environments lack audio
      }
      finally
      {
        Interlocked.Exchange(ref _isBeeping, false);
      }
    });

    return Task.CompletedTask;
  }
}
