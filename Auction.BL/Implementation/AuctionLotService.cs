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
                try
                {
                    link = await _cloudinaryService.UploadImageAsync(file);
                    if (link is null)
                    {
                        _logger.LogWarning(
                            "Image upload skipped or failed for lot creation. FileName: {FileName}, Account: {UserId}",
                            file.FileName,
                            account.Id);
                    }
                }
                catch (Exception uploadException)
                {
                    _logger.LogWarning(
                        uploadException,
                        "Image upload failed, continuing lot creation without image. FileName: {FileName}, Account: {UserId}",
                        file.FileName,
                        account.Id);
                }
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
            return Result<AuctionLotDtoOutput>.Failure(e.Message);
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
            if (lotById.OwnerId != account.Id)
            {
                throw new Exception($"You are not the owner of this auction lot. Id: {lotId}, Account: {account}");
            }

            var openWithoutBids = lotById.Status == Status.Open &&
                                  lotById.CurrentWinnerId is null;

            if (lotById.Status != Status.Active && lotById.Status != Status.Cancelled && !openWithoutBids)
            {
                throw new Exception($"You cannot update this auction lot because its status is {lotById.Status}. Id: {lotId}, Account: {account}");
            }
            
            var canChangeStartTime = lotById.Status == Status.Active || lotById.Status == Status.Cancelled;
            if (canChangeStartTime && lotById.StartTime != lotDtoInput.StartTime)
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
            return Result<AuctionLotDtoOutput>.Failure(e.Message);
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

            if (lotById.OwnerId != account.Id)
            {
                throw new Exception($"You are not the owner of this auction lot. Id: {lotId}, Account: {account}");
            }

            var openWithoutBids = lotById.Status == Status.Open &&
                                  lotById.CurrentWinnerId is null;

            if (lotById.Status != Status.Active && lotById.Status != Status.Cancelled && !openWithoutBids)
            {
                throw new Exception($"You cannot delete this auction lot because its status is {lotById.Status}. Id: {lotId}, Account: {account}");
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
            return Result<AuctionLotDtoOutput>.Failure(e.Message);
        }    
    }

    public async Task<Result<AuctionLotDtoOutput>> MarkAuctionLotAsDelivered(Guid id)
    {
        try
        {
            var lotById = await _lotRepository.GetLot(id);
            if (lotById is null)
            {
                throw new Exception($"Error with finding auction lot by Id: {id}");
            }

            if (lotById.Status != Status.Sold)
            {
                throw new Exception($"You cannot mark this auction lot as delivered because its status is {lotById.Status}. Id: {id}");
            }

            lotById.Status = Status.Delivered;
            lotById.UpdatedAt = DateTime.UtcNow;
            var lot = await _lotRepository.ChangeLot(lotById);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with marking auction lot as delivered. Lot ID: {LotID}", id);
            return Result<AuctionLotDtoOutput>.Failure(e.Message);
        }
    }

    public async Task<Result<AuctionLotDtoOutput>> CancelAuctionLotDelivery(Guid id)
    {
        try
        {
            var lotById = await _lotRepository.GetLot(id);
            if (lotById is null)
            {
                throw new Exception($"Error with finding auction lot by Id: {id}");
            }

            if (lotById.Status != Status.Delivered)
            {
                throw new Exception($"You cannot cancel delivery for this auction lot because its status is {lotById.Status}. Id: {id}");
            }

            lotById.Status = Status.Sold;
            lotById.UpdatedAt = DateTime.UtcNow;
            var lot = await _lotRepository.ChangeLot(lotById);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with cancelling delivered state for auction lot. Lot ID: {LotID}", id);
            return Result<AuctionLotDtoOutput>.Failure(e.Message);
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

    public List<AuctionLotDtoOutput> GetListOfArchivedAuctionLots()
    {
        return _lotRepository.GetArchivedLot()!
            .Select(AuctionLotMapping.ToDto)
            .ToList();
    }

    public List<AuctionLotDtoOutput> GetWonLotsByUserId(string userId)
    {
        return _lotRepository.GetWonLotsByWinnerId(userId)!
            .Select(AuctionLotMapping.ToDto)
            .ToList();
    }
}
