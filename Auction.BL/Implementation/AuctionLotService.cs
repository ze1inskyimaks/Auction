using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Mapping;
using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.Extensions.Logging;

namespace Auction.BL.Implementation;

public class AuctionLotService : IAuctionLotService
{
    private readonly IAuctionLotRepository _lotRepository;
    private readonly ILogger<AuctionLotService> _logger;

    public AuctionLotService(IAuctionLotRepository lotRepository, ILogger<AuctionLotService> logger)
    {
        _lotRepository = lotRepository;
        _logger = logger;
    }
    public async Task<bool> CreateAuctionLot(AuctionLotDtoInput lot, Account account)
    {
        try
        { 
            await _lotRepository.CreateLot(AuctionLotMapping.ToAuctionLot(lot, account));
            _logger.LogInformation("Successful creating auction lot with account: {UserId}", account);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with creating auction lot with account: {UserID}", account);
            return false;
        }
    }

    public async Task<bool> ChangeAuctionLot(AuctionLotDtoInput lot, Account account)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAuctionLot(Guid id, Account account)
    {
        throw new NotImplementedException();
    }

    public async Task GetAuctionLot(Guid id)
    {
        throw new NotImplementedException();
    }
}