using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Services;
using Auction.Data;
using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Auction.API.ApiControllers;

[ApiController]
[Route("api/v1/auction")]
public class AuctionApi : ControllerBase
{
    private readonly UserManager<Account> _userManager;
    private readonly IAuctionLotService _lotService;
    private readonly IAuctionHistoryRepository _historyRepository;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ICacheService _cacheService;
    private readonly AppDbContext _dbContext;
    private const string Lobby = "lobby";

    public AuctionApi(
        UserManager<Account> userManager,
        IAuctionLotService lotService,
        IAuctionHistoryRepository historyRepository,
        IHubContext<AuctionHub> hubContext,
        ICacheService cacheService,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _lotService = lotService;
        _historyRepository = historyRepository;
        _hubContext = hubContext;
        _cacheService = cacheService;
        _dbContext = dbContext;
    }
    
    [Authorize(Roles = "USER")]
    [HttpPost]
    public async Task<IActionResult> CreateAuctionLot([FromForm] AuctionLotDtoInput lotInput, [FromForm] IFormFile? file)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.CreateAuctionLot(lotInput, user, file);
        if (result.IsFailure)
        {
            return MapLotError(result.Error, "Error with creating auction lot");
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby).SendAsync("ReceiveNewLot", new
        {
            lot!.Id,
            lot.Name,
            lot.StartPrice
        });
        
        _ = _cacheService.CacheActiveAuctionLotsAsync();
        
        return Ok(lot.Id);
    }
    
    [Authorize(Roles = "USER")]
    [HttpPut]
    public async Task<IActionResult> ChangeAuctionLot([FromBody] AuctionLotDtoInput lotInput, Guid lotId)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.ChangeAuctionLot(lotId, lotInput, user);
        if (result.IsFailure)
        {
            return MapLotError(result.Error, "Error with changing auction lot");
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby, lot!.Id.ToString()).SendAsync("ReceiveNewLot", new
        {
            lot.Id,
            lot.Name,
            lot.StartPrice
        }); 

        _ = _cacheService.CacheActiveAuctionLotsAsync();
        
        return Ok(lot.Id);
    }
    
    [Authorize(Roles = "USER")]
    [HttpDelete]
    public async Task<IActionResult> DeleteAuctionLot(Guid lotId)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.DeleteAuctionLot(lotId, user);
        if (result.IsFailure)
        {
            return MapLotError(result.Error, "Error with deleting auction lot");
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby, lot!.Id.ToString()).SendAsync("ReceiveDeletedLot", new
        {
            lot.Id
        }); 

        _ = _cacheService.CacheActiveAuctionLotsAsync();
        
        return NoContent();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/deliver")]
    public async Task<IActionResult> MarkAuctionLotAsDelivered(Guid id)
    {
        var result = await _lotService.MarkAuctionLotAsDelivered(id);
        if (result.IsFailure)
        {
            return MapLotError(result.Error, "Error with marking lot as delivered");
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}/undeliver")]
    public async Task<IActionResult> CancelAuctionLotDelivery(Guid id)
    {
        var result = await _lotService.CancelAuctionLotDelivery(id);
        if (result.IsFailure)
        {
            return MapLotError(result.Error, "Error with cancelling lot delivery");
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionLotById(Guid id)
    {
        var result = await _lotService.GetAuctionLot(id);
        return Ok(result);
    }

    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetAuctionLotImage(Guid id)
    {
        var image = await _dbContext.AuctionLotImages
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.LotId == id);

        if (image is null || image.Data.Length == 0)
        {
            return NotFound(new { message = "Image not found." });
        }

        return File(image.Data, image.ContentType, enableRangeProcessing: true);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetAuctionLotHistory(Guid id)
    {
        var history = await _historyRepository.GetHistoryLogsByLotId(id);
        var response = history.Select(h => new
        {
            h.Id,
            h.LotId,
            h.HistoryNumber,
            h.BidderId,
            h.BidAmount,
            h.BidTime
        });
        return Ok(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetActiveLot()
    {
        var data = await _cacheService.GetCachedActiveAuctionLotsAsync();
        if (data is not null)
        {
            return Ok(data);
        }
        _ = _cacheService.CacheActiveAuctionLotsAsync();
        var activeList = _lotService.GetListOfActiveAuctionLots();
        return Ok(activeList);
    }

    [HttpGet("history")]
    public IActionResult GetArchivedLots()
    {
        var archivedList = _lotService.GetListOfArchivedAuctionLots();
        return Ok(archivedList);
    }

    [Authorize(Roles = "USER")]
    [HttpGet("my/history/bids")]
    public async Task<IActionResult> GetMyBidHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(user.Id, out var bidderId))
        {
            return BadRequest(new { message = "Invalid user id format." });
        }

        var history = await _historyRepository.GetHistoryLogsByBidderId(bidderId);
        var response = history.Select(h => new
        {
            h.Id,
            h.LotId,
            LotName = h.AuctionLot.Name,
            LotStatus = h.AuctionLot.Status,
            LotWinnerId = h.AuctionLot.WinnerId,
            LotEndPrice = h.AuctionLot.EndPrice,
            h.HistoryNumber,
            h.BidderId,
            h.BidAmount,
            h.BidTime
        });

        return Ok(response);
    }

    [Authorize(Roles = "USER")]
    [HttpGet("my/history/wins")]
    public async Task<IActionResult> GetMyWinsHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var wins = _lotService.GetWonLotsByUserId(user.Id);
        return Ok(wins);
    }

    private IActionResult MapLotError(string error, string fallbackMessage)
    {
        if (error.Contains("not the owner of this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("not owned this auction lot", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        if (error.Contains("can`t change this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("cannot update this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("cannot delete this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("cannot mark this auction lot as delivered", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("cannot cancel delivery for this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("selected category does not exist or is inactive", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("image file", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("image format", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("invalid image", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("can't update this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("can't delete this auction lot", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("Error with finding auction lot by Id", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = error });
        }

        return Problem(detail: $"{fallbackMessage}: {error}", statusCode: StatusCodes.Status500InternalServerError);
    }
}

//TODO: Need to add photo property to Auction Lot
//TODO: Need to add elastic search or something else
//TODO: Need to add pay system
