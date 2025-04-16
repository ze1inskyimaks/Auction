using Auction.BL.Interface;
using Microsoft.AspNetCore.SignalR;

namespace Auction.BL.Services;

public class AuctionHub : Hub
{
    private readonly IAuctionLobbyService _lotService;
    private const string Lobby = "lobby";

    public AuctionHub(IAuctionLobbyService lotService)
    {
        _lotService = lotService;
    }
    
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var lotId = httpContext.Request.Query["lotId"];

        if (!string.IsNullOrEmpty(lotId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, lotId!);
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Lobby);
        }

        await base.OnConnectedAsync();
    }
    
    

    public async Task PlaceBid(Guid lotId, string accountId, double amount)
    {
        await _lotService.Bid();
        //TODO: Need to create some logic before send message  
        await Clients.Groups(lotId.ToString()).SendAsync("ReceiveBid", lotId, accountId, amount);
    }

    public async Task CreatingAuctionLot(Guid lotId, string name, double startPrice)
    {
        await Clients.Groups(Lobby).SendAsync("ReceiveNewLot", new
        {
            LotId = lotId,
            Name = name,
            StartPrice = startPrice
        });
    }
}