using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
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

    public AuctionApi(UserManager<Account> userManager, IAuctionLotService lotService)
    {
        _userManager = userManager;
        _lotService = lotService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateAuctionLot([FromBody] AuctionLotDtoInput lotInput)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
            return Unauthorized();

        var result = await _lotService.CreateAuctionLot(lotInput, user);
        if (!result)
        {
            return Problem("Error with creating auction lot!");
        }

        return Ok();
    }
}