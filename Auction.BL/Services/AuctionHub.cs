using System.Security.Claims;
using Auction.BL.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auction.BL.Services;

public class AuctionHub : Hub
{
    private readonly ILogger<AuctionHub> _logger;
    private readonly IAuctionLobbyService _lobbyService;
    private readonly IAuctionTimerService _timerService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IHubContext<AuctionHub> _hubContext;
    private const string Lobby = "lobby";

    public AuctionHub(ILogger<AuctionHub> logger, 
                      IAuctionLobbyService lobbyService, 
                      IAuctionTimerService timerService,
                      IServiceScopeFactory serviceScopeFactory,
                      IHubContext<AuctionHub> hubContext            )
    {
        _logger = logger;
        _lobbyService = lobbyService;
        _timerService = timerService;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var lotId = httpContext!.Request.Query["lotId"];

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

    public async Task PlaceBid(Guid lotId, double amount)
    {
        var accountId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(accountId) || !Guid.TryParse(accountId, out var userId))
        {
            await Clients.Caller.SendAsync("Error", "Invalid account ID.");
            return;
        }

        try
        {
            // Робимо ставку через основний сервіс
            await _lobbyService.Bid(lotId, userId, amount);
            await Clients.Groups(lotId.ToString()).SendAsync("ReceiveBid", lotId, accountId, amount);

            _timerService.StartTimer(lotId, async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var scopedLobbyService = scope.ServiceProvider.GetRequiredService<IAuctionLobbyService>();

                    await scopedLobbyService.FinishAuction(lotId, userId, amount);

                    await _hubContext.Clients.Groups(lotId.ToString(), Lobby)
                        .SendAsync("ReceiveFinishLot", lotId, accountId, amount);

                    var scopedCache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                    await scopedCache.CacheActiveAuctionLotsAsync();
                    
                    _logger.LogInformation($"Аукціон для лота {lotId} успішно завершено з користувачем {accountId}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Помилка у зворотному виклику таймера при завершенні аукціонного лота з обліковим записом: {UserID}", accountId);
                    throw;
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Помилка при ставці або завершенні аукціону для лота: {LotID} з обліковим записом: {UserID}", lotId, accountId);
            await Clients.Caller.SendAsync("Error", $"Помилка: {e.Message} з лотом: {lotId}");
        }
    }
}
