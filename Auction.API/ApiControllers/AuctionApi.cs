using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Services;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Auction.API.ApiControllers;

[ApiController]
[Route("api/v1/auction")]
public class AuctionApi : ControllerBase
{
    private readonly UserManager<Account> _userManager;
    private readonly IAuctionLotService _lotService;
    private readonly IHubContext<AuctionHub> _hubContext;
    private const string Lobby = "lobby";

    public AuctionApi(UserManager<Account> userManager, IAuctionLotService lotService, IHubContext<AuctionHub> hubContext)
    {
        _userManager = userManager;
        _lotService = lotService;
        _hubContext = hubContext;
    }
    
    [Authorize(Roles = "USER")]
    [HttpPost]
    public async Task<IActionResult> CreateAuctionLot([FromBody] AuctionLotDtoInput lotInput)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.CreateAuctionLot(lotInput, user);
        if (result.IsFailure)
        {
            return Problem("Error with creating auction lot: {error}", result.Error);
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby).SendAsync("ReceiveNewLot", new
        {
            lot!.Id,
            lot.Name,
            lot.StartPrice
        });
        return Ok();
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
            return Problem("Error with creating auction lot: {error}", result.Error);
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby, lot!.Id.ToString()).SendAsync("ReceiveNewLot", new
        {
            lot.Id,
            lot.Name,
            lot.StartPrice
        }); 

        return Ok();
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
            return Problem("Error with creating auction lot: {error}", result.Error);
        }

        var lot = result.Value;
        await _hubContext.Clients.Groups(Lobby, lot!.Id.ToString()).SendAsync("ReceiveDeletedLot", new
        {
            lot.Id
        }); 

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionLotById(Guid id)
    {
        var result = await _lotService.GetAuctionLot(id);
        return Ok(result);
    }
    
    [HttpGet]
    public IActionResult GetActiveLot()
    {
        var activeList = _lotService.GetListOfActiveAuctionLots();

        return Ok(activeList);
    }
}

//TODO: Add a cache like a redis in GetActiveAuctionLot function
//TODO: Set all time in utsnow
//TODO: Need to add frontend part(blazor page with signalR integration)
//TODO: Need to add photo property to Auction Lot
//TODO: Need to add elastic search or something else
//TODO: Need to add all dependency injection system
//TODO: Need to add pay system