using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Result;
using Auction.Data.Model;

namespace Auction.BL.Interface;

public interface IAuctionLotService
{
    public Task<Result> CreateAuctionLot(AuctionLotDtoInput lot, Account account);
    public Task<bool> ChangeAuctionLot(AuctionLotDtoInput lot, Account account);
    public Task<bool> DeleteAuctionLot(Guid id, Account account);
    public Task GetAuctionLot(Guid id);
}