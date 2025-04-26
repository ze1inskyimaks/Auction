using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Services;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auction.API.ApiControllers;

[Authorize]
[ApiController]
[Route("api/v1/auction")]
public class AuctionApi : ControllerBase
{
    private readonly UserManager<Account> _userManager;
    private readonly IAuctionLotService _lotService;
    private readonly AuctionHub _auctionHub;

    public AuctionApi(UserManager<Account> userManager, IAuctionLotService lotService, AuctionHub auctionHub)
    {
        _userManager = userManager;
        _lotService = lotService;
        _auctionHub = auctionHub;
    }
    
    [Authorize(Roles = "USER")]
    [HttpPost]
    public async Task<IActionResult> CreateAuctionLot([FromBody] AuctionLotDtoInput lotInput)
    {
        //TODO: Need to add photo property to Auction Lot
        //TODO: Need to add elastic search or something else
        //TODO: Need to add all dependency injection system
        //TODO: Need to add pay system
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.CreateAuctionLot(lotInput, user);
        if (result.IsFailure)
        {
            return Problem("Error with creating auction lot: {error}", result.Error);
        }

        var lot = result.Value;
        await _auctionHub.CreateAuctionLot(lot!); 

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
        await _auctionHub.ChangeAuctionLot(lot!); 

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
        await _auctionHub.DeleteAuctionLot(lot!); 

        return Ok();
    }
}