namespace Auction.BL.Interface;

public interface IAuctionTimerService
{
    public void StartTimer(Guid lotId, Func<Task> onTimerEnd, TimeSpan? delay = null);
    public void StopTimer(Guid lotId);
}