using System.Collections.Concurrent;
using Auction.BL.Interface;

namespace Auction.BL.Services;

public class AuctionTimerService : IAuctionTimerService
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _timers = new();
    private readonly TimeSpan _defaultDelay = TimeSpan.FromSeconds(15);
    
    public void StartTimer(Guid lotId, Func<Task> onTimerEnd, TimeSpan? delay = null)
    {
        StopTimer(lotId);

        var cts = new CancellationTokenSource();
        _timers[lotId] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay ?? _defaultDelay, cts.Token);
                if (!cts.Token.IsCancellationRequested)
                {
                    _timers.TryRemove(lotId, out _);
                    await onTimerEnd();
                }
            }
            catch (TaskCanceledException) { }
        });
    }

    public void StopTimer(Guid lotId)
    {
        if (_timers.TryRemove(lotId, out var cts))
        {
            cts.Cancel();
        }
    }
}