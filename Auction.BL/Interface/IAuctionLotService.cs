using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Result;
using Auction.Data.Model;
using Microsoft.AspNetCore.Http;

namespace Auction.BL.Interface;

public interface IAuctionLotService
{
    public Task<Result<AuctionLotDtoOutput>> CreateAuctionLot(AuctionLotDtoInput lot, Account account, IFormFile? file = null);
    public Task<Result<AuctionLotDtoOutput>> ChangeAuctionLot(Guid lotId, AuctionLotDtoInput lot, Account account);
    public Task<Result<AuctionLotDtoOutput>> DeleteAuctionLot(Guid id, Account account);
    public Task<AuctionLotDtoOutput?> GetAuctionLot(Guid id);
    public List<AuctionLotDtoOutput>? GetListOfActiveAuctionLots();
}