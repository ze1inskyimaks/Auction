using Auction.BL.Interface;
using Auction.BL.Services;
using Auction.Data;
using Auction.Data.Implementation;
using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Auction.BL.Implementation;

public class AuctionLobbyService : IAuctionLobbyService
{
    private readonly IAuctionLotRepository _lotRepository;
    private readonly IAuctionHistoryRepository _historyRepository;
    private readonly UserManager<Account> _userManager;
    private readonly AppDbContext _context;
    private readonly IHubContext<AuctionHub> _hubContext;
    private const string Lobby = "lobby";

    public AuctionLobbyService(IAuctionLotRepository lotRepository, IAuctionHistoryRepository historyRepository, UserManager<Account> userManager, AppDbContext context, IHubContext<AuctionHub> hubContext)
    {
        _lotRepository = lotRepository;
        _historyRepository = historyRepository;
        _userManager = userManager;
        _context = context;
        _hubContext = hubContext;
    }
    public async Task StartAuction(Guid lotId)
    {
        var lot = await _lotRepository.GetLot(lotId);
        if (lot is null)
        {
            throw new Exception($"Error with finding auction lot by id: {lotId}");
        }

        if (lot.Status != Status.Active)
        {
            throw new Exception($"Error with status in auction lot by id: {lotId}, his status: {lot.Status}");
        }

        lot.Status = Status.Open;
        await _lotRepository.ChangeLot(lot);

        await _hubContext.Clients.Groups(Lobby, lot.Id.ToString())
            .SendAsync("ReceiveStartOfAuction", lotId);
    }

    public async Task Bid(Guid lotId, Guid accountId, double amount)
    {
        var lot = await _lotRepository.GetLot(lotId);
        if (lot is null)
        {
            throw new Exception($"Error with finding auction lot by id: {lotId}");
        }

        if (lot.Status != Status.Open)
        {
            throw new Exception($"Error with status in auction lot by id: {lotId}, his status: {lot.Status}");
        }

        if (lot.StartPrice > amount || lot.CurrentPrice > amount)
        {
            throw new Exception(
                $"Error with amount of price, because you send: {amount}, " +
                $"but starter price {lot.StartPrice} or current price {lot.CurrentPrice} bigger!");
        }

        if (lot.OwnerId == accountId.ToString() || lot.CurrentWinnerId == accountId)
        {
            throw new Exception(
                $"Error with account id: {accountId}, because owner of lot {lot.OwnerId} " +
                $"or current winner id {lot.CurrentWinnerId} the same!");
            
        }

        var latestHistoryNumber = await _historyRepository.GetLastHistoryNumberByLotId(lotId);

        if (latestHistoryNumber is null)
        {
            throw new Exception("Error with history number, his is null.");
        }

        var newHistoryNumber = latestHistoryNumber + 1;
        var historyLog = new AuctionHistory()
        {
            LotId = lot.Id,
            AuctionLot = lot,
            HistoryNumber = newHistoryNumber,
            BidderId = accountId,
            BidAmount = amount
        };
        var log = await _historyRepository.CreateHistoryLog(historyLog);

        lot.CurrentWinnerId = accountId;
        lot.CurrentPrice = amount;
        lot.LastBitTime = DateTime.Now;
        lot.AuctionHistoryId!.Add(log.Id);
        lot.AuctionHistories!.Add(log);
        
        await _lotRepository.ChangeLot(lot);
    }

    public async Task FinishAuction(Guid lotId, Guid accountId, double amount)
    {
        var lot = await _lotRepository.GetLot(lotId);
        if (lot is null)
        {
            throw new Exception($"Error with finding auction lot by id: {lotId}");
        }

        if (lot.Status != Status.Open)
        {
            throw new Exception($"Error with status in auction lot by id: {lotId}, his status: {lot.Status}");
        }

        var account = await _userManager.FindByIdAsync(accountId.ToString());

        lot.EndPrice = amount;
        lot.WinnerId = accountId.ToString();
        lot.WinnerAccount = account;
        lot.Status = Status.Sold;

        await _lotRepository.ChangeLot(lot);

        await _userManager.AddWonLotToAccount(_context, accountId.ToString(), lot);
    }
}