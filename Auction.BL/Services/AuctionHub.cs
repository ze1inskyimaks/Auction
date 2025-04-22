using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Auction.BL.Services;

public class AuctionHub : Hub
{
    private readonly ILogger<AuctionHub> _logger;
    private readonly IAuctionLobbyService _lobbyService;
    private readonly IAuctionLotService _lotService;
    private readonly IAuctionTimerService _timerService;
    private const string Lobby = "lobby";

    public AuctionHub(ILogger<AuctionHub> logger, IAuctionLobbyService lobbyService, IAuctionLotService lotService, IAuctionTimerService timerService)
    {
        _logger = logger;
        _lobbyService = lobbyService;
        _lotService = lotService;
        _timerService = timerService;
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
        try
        {
            await _lobbyService.Bid(lotId, Guid.Parse(accountId), amount);
            await Clients.Groups(lotId.ToString()).SendAsync("ReceiveBid", lotId, accountId, amount);
        
            _timerService.StartTimer(lotId, async () =>
            {
                await _lobbyService.FinishAuction(lotId, Guid.Parse(accountId), amount);
                await Clients.Groups(lotId.ToString(), Lobby).SendAsync("ReceiveFinishLot", lotId, accountId, amount);
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with deleting auction lot with account: {UserID}", accountId);
            await Clients.Caller.SendAsync($"Error: {e} with lot id: {lotId}");
        }
    }
}
