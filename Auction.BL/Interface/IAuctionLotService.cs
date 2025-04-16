using Auction.BL.Model.AuctionLot;
using Auction.Data.Model;

namespace Auction.BL.Interface;

public interface IAuctionLotService
{
    public Task<bool> CreateAuctionLot(AuctionLotDtoInput lot, Account account);
    public Task<bool> ChangeAuctionLot(AuctionLotDtoInput lot, Account account);
    public Task<bool> DeleteAuctionLot(Guid id, Account account);
    public Task GetAuctionLot(Guid id);
}