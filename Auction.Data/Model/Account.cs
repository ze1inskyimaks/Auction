using Microsoft.AspNetCore.Identity;

namespace Auction.Data.Model;

public class Account : IdentityUser
{
    public List<Guid>? HostedLotIds { get; set; }
    public List<AuctionLot>? HostedLots { get; set; }

    public List<Guid>? WinningLotIds { get; set; }
    public List<AuctionLot>? WinningLots { get; set; }
}