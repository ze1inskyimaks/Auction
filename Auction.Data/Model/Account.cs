using Microsoft.AspNetCore.Identity;

namespace Auction.Data.Model;

public class Account : IdentityUser
{
    public List<AuctionLot>? HostedLots { get; set; } = new List<AuctionLot>();

    public List<AuctionLot>? WinningLots { get; set; } = new List<AuctionLot>();
}