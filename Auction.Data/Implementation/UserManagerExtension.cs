using Auction.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data.Implementation;

public static class UserManagerExtension
{
    public static async Task<Account?> FindByIdWithHostedLotsAsync(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId)
    {
        return await context.Users
            .Include(u => u.HostedLots)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public static async Task<Account?> FindByIdWithWonLotsAsync(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId)
    {
        return await context.Users
            .Include(u => u.WinningLots)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public static async Task AddHostedLotToAccount(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId,
        AuctionLot lot
        )
    {
        var acc = await userManager.FindByIdWithHostedLotsAsync(context, userId);
        if (acc is null)
            return;

        lot.OwnerId = acc.Id;
        acc.HostedLots?.Add(lot);

        context.AuctionLots.Add(lot);
        await context.SaveChangesAsync();
    }
    
    public static async Task AddWonLotToAccount(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId,
        AuctionLot lot
    )
    {
        var acc = await userManager.FindByIdWithHostedLotsAsync(context, userId);
        if (acc is null)
            return;

        lot.OwnerId = acc.Id;
        acc.WinningLots?.Add(lot);

        context.AuctionLots.Add(lot);
        await context.SaveChangesAsync();
    }
    
    public static async Task RemoveHostedLotFromAccountAsync(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId,
        Guid lotId
    )
    {
        var acc = await userManager.FindByIdWithHostedLotsAsync(context, userId);
        if (acc is null)
            return;

        var lot = acc.HostedLots?.FirstOrDefault(l => l.Id == lotId);
        if (lot is not null)
        {
            acc.HostedLots!.Remove(lot);
            lot.OwnerId = String.Empty;            // або null, якщо тип змінний
            lot.OwnerAccount = null;

            await context.SaveChangesAsync();
        }
    }

    public static async Task RemoveWonLotFromAccountAsync(
        this UserManager<Account> userManager,
        AppDbContext context,
        string userId,
        Guid lotId
    )
    {
        var acc = await userManager.FindByIdWithWonLotsAsync(context, userId);
        if (acc is null)
            return;

        var lot = acc.WinningLots?.FirstOrDefault(l => l.Id == lotId);
        if (lot is not null)
        {
            acc.WinningLots!.Remove(lot);
            lot.WinnerId = null;
            lot.WinnerAccount = null;

            await context.SaveChangesAsync();
        }
    }

}