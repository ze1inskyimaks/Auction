using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Mapping;
using Auction.BL.Model.Result;
using Auction.Data;
using Auction.Data.Implementation;
using Auction.Data.Interface;
using Auction.Data.Model;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Auction.BL.Implementation;

public class AuctionLotService : IAuctionLotService
{
    private readonly IAuctionLotRepository _lotRepository;
    private readonly ILogger<AuctionLotService> _logger;
    private readonly IAuctionLobbyService _lobbyService;
    private readonly UserManager<Account> _userManager;
    private readonly AppDbContext _context;
    private readonly CloudinaryService _cloudinaryService;

    public AuctionLotService(IAuctionLotRepository lotRepository, ILogger<AuctionLotService> logger, IAuctionLobbyService lobbyService, UserManager<Account> userManager, AppDbContext context, CloudinaryService cloudinaryService)
    {
        _lotRepository = lotRepository;
        _logger = logger;
        _lobbyService = lobbyService;
        _userManager = userManager;
        _context = context;
        _cloudinaryService = cloudinaryService;
    }
    public async Task<Result<AuctionLotDtoOutput>> CreateAuctionLot(AuctionLotDtoInput lotDtoInput, Account account, IFormFile? file = null)
    {
        try
        {
            string? link = null;
            if (file is not null)
            {
                link = await _cloudinaryService.UploadImageAsync(file);
            }

            if (link is null)
            {
                throw new Exception("Error with uploading file");
            }
            
            var lot = await _lotRepository.CreateLot(AuctionLotMapping.ToAuctionLot(lotDtoInput, account, link));
            
            var jobId = BackgroundJob.Schedule(
                () => _lobbyService.StartAuction(lot.Id), 
                lot.StartTime - DateTime.UtcNow
            );
            lot.JobId = jobId;

            await _lotRepository.ChangeLot(lot);

            await _userManager.AddHostedLotToAccount(_context, account.Id, lot);
            
            _logger.LogInformation("Successful creating auction lot with account: {UserId}", account);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with creating auction lot with account: {UserID}", account);
            return Result<AuctionLotDtoOutput>.Failure(e.ToString());
        }
    }

    public async Task<Result<AuctionLotDtoOutput>> ChangeAuctionLot(Guid lotId, AuctionLotDtoInput lotDtoInput, Account account)
    {
        try
        {
            var lotById = await _lotRepository.GetLot(lotId);
            if (lotById is null)
            {
                throw new Exception($"Error with finding auction lot by Id: {lotId}");
            }
            if (lotById.OwnerAccount != account)
            {
                throw new Exception($"You are not owned this auction lot. Id: {lotId}, Account: {account}");
            }

            if (lotById.Status != Status.Active && lotById.Status != Status.Cancelled)
            {
                throw new Exception($"You are can`t change this auction lot, because his status: {lotById.Status}. Id: {lotId}, Account: {account}");
            }
            
            if (lotById.StartTime != lotDtoInput.StartTime)
            {
                if (!string.IsNullOrEmpty(lotById.JobId))
                {
                    BackgroundJob.Delete(lotById.JobId);
                }

                var newJobId = BackgroundJob.Schedule(
                    () => _lobbyService.StartAuction(lotId),
                    lotDtoInput.StartTime - DateTime.UtcNow
                );

                lotById.JobId = newJobId;
                lotById.StartTime = lotDtoInput.StartTime;
            }
            
            lotById.Name = lotDtoInput.Name;
            lotById.Description = lotDtoInput.Description;
            lotById.StartPrice = lotDtoInput.StartPrice;
            lotById.UpdatedAt = DateTime.UtcNow;    
            
            var lot = await _lotRepository.ChangeLot(lotById);
            
            _logger.LogInformation("Successful changing auction lot with account: {UserId}", account);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with changing auction lot with account: {UserID}", account);
            return Result<AuctionLotDtoOutput>.Failure(e.ToString());
        }
    }

    public async Task<Result<AuctionLotDtoOutput>> DeleteAuctionLot(Guid lotId, Account account)
    {
        try
        {
            var lotById = await _lotRepository.GetLot(lotId);
            if (lotById is null)
            {
                throw new Exception($"Error with finding auction lot by Id: {lotId}");
            }

            if (lotById.OwnerAccount != account)
            {
                throw new Exception($"You are not owned this auction lot. Id: {lotId}, Account: {account}");
            }
            if (lotById.Status != Status.Active && lotById.Status != Status.Cancelled)
            {
                throw new Exception($"You are can`t change this auction lot, because his status: {lotById.Status}. Id: {lotId}, Account: {account}");
            }
            
            var lot = await _lotRepository.DeleteLot(lotById);
            
            if (!string.IsNullOrEmpty(lotById.JobId))
            {
                BackgroundJob.Delete(lotById.JobId);
            }
            
            _logger.LogInformation("Successful deleting auction lot with account: {UserId}", account);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with deleting auction lot with account: {UserID}", account);
            return Result<AuctionLotDtoOutput>.Failure(e.ToString());
        }    
    }

    public async Task<AuctionLotDtoOutput?> GetAuctionLot(Guid id)
    {
        var lot = await _lotRepository.GetLot(id);
        if (lot is null)
        {
            return null;
        }
        return AuctionLotMapping.ToDto(lot);
    }
    
    public List<AuctionLotDtoOutput> GetListOfActiveAuctionLots()
    {
        return _lotRepository.GetActiveLot()!
            .Select(AuctionLotMapping.ToDto)
            .ToList();
    }
}