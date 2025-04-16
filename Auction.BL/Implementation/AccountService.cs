using Auction.BL.Model.Account;
using Auction.BL.Model.Account.DTO;
using Auction.BL.Model.Mapping;
using Auction.Data;
using Auction.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace Auction.BL.Implementation;

public class AccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly JwtService _jwtService;
    private readonly AppDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountService(UserManager<Account> userManager, JwtService jwtService, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<string?> Login(AccountDTO accountDto)
    {
        // Знайти користувача за його ім'ям
        var acc = await _userManager.FindByEmailAsync(accountDto.Email);
        if (acc == null)
        {
            throw new Exception("User indefinite!");
        }

        // Перевірка пароля для знайденого користувача
        var passwordValid = await _userManager.CheckPasswordAsync(acc, accountDto.PasswordHash);
        if (!passwordValid)
        {
            throw new Exception("Wrong password");
        }

        return await _jwtService.GenerateJwtToken(acc);
    }

    public async Task Register(AccountDTO accountDto, uint role)
    {
        if (await _userManager.FindByEmailAsync(accountDto.Email) != null)
        {
            throw new Exception("Користувач із таким емейлом вже існує.");
        }
        
        var account = AccountMapping.ToModel(accountDto);

        var result = await _userManager.CreateAsync(account, accountDto.PasswordHash);
        if (!result.Succeeded)
        {
            throw new Exception("Не вдалося створити користувача: " +
                                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(account, Role.USER);
    }

    public async Task AddRoleToAccount(string id, string role)
    {
        role = role.ToUpper();
        var roleExist = await _roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            throw new Exception("Не вдалося знайти роль " + role);
        }

        var acc = await _userManager.FindByIdAsync(id);
        if (acc == null)
        {
            throw new Exception("Account not been found!");
        }
        
        await _userManager.AddToRoleAsync(acc, role);
    }
    
    public async Task RemoveRoleToAccount(string id, string role)
    {
        role = role.ToUpper();
        var roleExist = await _roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            throw new Exception("Не вдалося знайти роль " + role);
        }

        var acc = await _userManager.FindByIdAsync(id);
        if (acc == null)
        {
            throw new Exception("Account not been found!");
        }
        
        await _userManager.RemoveFromRoleAsync(acc, role);
    }
}