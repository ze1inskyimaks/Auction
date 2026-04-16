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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace Auction.BL.Implementation;

public class AuctionLotService : IAuctionLotService
{
    private readonly IAuctionLotRepository _lotRepository;
    private readonly ILogger<AuctionLotService> _logger;
    private readonly IAuctionLobbyService _lobbyService;
    private readonly IAuctionCategoryRepository _categoryRepository;
    private readonly UserManager<Account> _userManager;
    private readonly AppDbContext _context;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    public AuctionLotService(
        IAuctionLotRepository lotRepository,
        ILogger<AuctionLotService> logger,
        IAuctionLobbyService lobbyService,
        IAuctionCategoryRepository categoryRepository,
        UserManager<Account> userManager,
        AppDbContext context)
    {
        _lotRepository = lotRepository;
        _logger = logger;
        _lobbyService = lobbyService;
        _categoryRepository = categoryRepository;
        _userManager = userManager;
        _context = context;
    }
    public async Task<Result<AuctionLotDtoOutput>> CreateAuctionLot(AuctionLotDtoInput lotDtoInput, Account account, IFormFile? file = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (lotDtoInput.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetById(lotDtoInput.CategoryId.Value);
                if (category is null || !category.IsActive)
                {
                    throw new Exception("Selected category does not exist or is inactive.");
                }
            }

            string? link = null;
            
            var lot = await _lotRepository.CreateLot(AuctionLotMapping.ToAuctionLot(lotDtoInput, account, link));

            if (file is not null)
            {
                await SaveLotImageToDatabaseAsync(lot, file);
            }
            
            var jobId = BackgroundJob.Schedule(
                () => _lobbyService.StartAuction(lot.Id), 
                lot.StartTime - DateTime.UtcNow
            );
            lot.JobId = jobId;

            await _lotRepository.ChangeLot(lot);

            await _userManager.AddHostedLotToAccount(_context, account.Id, lot);

            await transaction.CommitAsync();
            
            _logger.LogInformation("Successful creating auction lot with account: {UserId}", account);
            return Result<AuctionLotDtoOutput>.Success(AuctionLotMapping.ToDto(lot));
        }
        catch (Exception e)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch
            {
                // ignore rollback failures, original exception is more important for API response
            }
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
            if (lotDtoInput.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetById(lotDtoInput.CategoryId.Value);
                if (category is null || !category.IsActive)
                {
                    throw new Exception("Selected category does not exist or is inactive.");
                }
            }
            lotById.CategoryId = lotDtoInput.CategoryId;
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

    public List<AuctionLotDtoOutput> GetHostedLotsByUserId(string userId)
    {
        return _lotRepository.GetHostedLotsByOwnerId(userId)!
            .Select(AuctionLotMapping.ToDto)
            .ToList();
    }

    private async Task SaveLotImageToDatabaseAsync(AuctionLot lot, IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new Exception("Image file is empty.");
        }

        if (file.Length > MaxImageSizeBytes)
        {
            throw new Exception("Image file is too large. Maximum allowed size is 5 MB.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new Exception("Unsupported image format. Allowed: JPEG, PNG, WEBP, GIF.");
        }

        await using var stream = file.OpenReadStream();
        var bytes = await ReadFullyAsync(stream, file.Length);
        if (!IsValidImageSignature(bytes, file.ContentType))
        {
            throw new Exception("Invalid image file content.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"{Guid.NewGuid():N}.bin";
        }

        var image = new AuctionLotImage
        {
            LotId = lot.Id,
            ContentType = file.ContentType,
            FileName = fileName,
            SizeBytes = bytes.Length,
            Data = bytes
        };

        await _context.AuctionLotImages.AddAsync(image);
        lot.LinkToImage = $"/api/v1/auction/{lot.Id}/image";
        await _lotRepository.ChangeLot(lot);
    }

    private static async Task<byte[]> ReadFullyAsync(Stream stream, long expectedLength)
    {
        if (expectedLength <= 0 || expectedLength > int.MaxValue)
        {
            using var dynamicMs = new MemoryStream();
            await stream.CopyToAsync(dynamicMs);
            return dynamicMs.ToArray();
        }

        var length = (int)expectedLength;
        var rented = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = await stream.ReadAsync(rented.AsMemory(totalRead, length - totalRead));
                if (read == 0)
                {
                    break;
                }
                totalRead += read;
            }

            return rented[..totalRead].ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static bool IsValidImageSignature(byte[] bytes, string contentType)
    {
        if (bytes.Length < 12)
        {
            return false;
        }

        if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[^2] == 0xFF && bytes[^1] == 0xD9;
        }

        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
        {
            return bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
        }

        if (contentType.Equals("image/gif", StringComparison.OrdinalIgnoreCase))
        {
            return bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38;
        }

        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            return bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                   bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;
        }

        return false;
    }
}
