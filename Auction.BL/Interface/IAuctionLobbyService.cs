namespace Auction.BL.Interface;

public interface IAuctionLobbyService
{
    public Task StartAuction(Guid lotId);
    public Task Bid(Guid lotId, Guid accountId, double amount);
    public Task FinishAuction(Guid lotId, Guid accountId, double amount);
}