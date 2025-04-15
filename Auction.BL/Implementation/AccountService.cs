using Auction.Data;
using Auction.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace Auction.BL.Implementation;

public class AccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly AppDbContext _context;

    public AccountService(UserManager<Account> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    
}