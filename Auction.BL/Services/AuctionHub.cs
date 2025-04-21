using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Microsoft.AspNetCore.SignalR;

namespace Auction.BL.Services;

public class AuctionHub : Hub
{
    private readonly IAuctionLobbyService _lobbyService;
    private readonly IAuctionLotService _lotService;
    private const string Lobby = "lobby";

    public AuctionHub(IAuctionLobbyService lobbyService, IAuctionLotService lotService)
    {
        _lobbyService = lobbyService;
        _lotService = lotService;
    }
    
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var lotId = httpContext!.Request.Query["lotId"];

        if (!string.IsNullOrEmpty(lotId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, lotId!);
            await SendAuctionLot(lotId!);
        }
        else
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Lobby);
            await SendActiveAuctionLot();
        }

        await base.OnConnectedAsync();
    }

    private async Task SendActiveAuctionLot()
    {
        await Clients.Groups(Lobby).SendAsync("GetActiveAuctionLot", _lotService.GetListOfActiveAuctionLots());
    }

    private async Task SendAuctionLot(string lotId)
    {
        await Clients.Groups(lotId).SendAsync("GetAuctionLot", await _lotService.GetAuctionLot(Guid.Parse(lotId)));
    }
    

    public async Task CreateAuctionLot(AuctionLotDtoOutput lot)
    {
        await Clients.Groups(Lobby).SendAsync("ReceiveNewLot", new
        {
            lot.Id,
            lot.Name,
            lot.StartPrice
        });
    }
    
    public async Task ChangeAuctionLot(AuctionLotDtoOutput lot)
    {
        await Clients.Groups(Lobby, lot.Id.ToString()).SendAsync("ReceiveNewLot", new
        {
            lot.Id,
            lot.Name,
            lot.StartPrice
        });
    }

    public async Task DeleteAuctionLot(AuctionLotDtoOutput lotId)
    {
        await Clients.Groups(Lobby, lotId.Id.ToString()).SendAsync("ReceiveDeletedLot", new
        {
            lotId.Id
        });
    }
    
    public async Task PlaceBid(Guid lotId, string accountId, double amount)
    {
        await _lobbyService.Bid();
        //TODO: Need to create some logic before send message  
        await Clients.Groups(lotId.ToString()).SendAsync("ReceiveBid", lotId, accountId, amount);
    }
}
