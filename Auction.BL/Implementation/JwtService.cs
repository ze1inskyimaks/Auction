using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auction.API.Options;
using Auction.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auction.BL.Implementation;

public class JwtService
{
    private readonly UserManager<Account> _userManager;
    private readonly string _key;
    private readonly int _expiresInHours;
    
    public JwtService(UserManager<Account> userManager, IOptions<JwtTokenOptions> tokenOptions)
    {
        _userManager = userManager;
        var options = tokenOptions.Value;
        _key = options.Key;
        _expiresInHours = options.ExpiresInHours;
    }
    
    public async Task<string> GenerateJwtToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var secretKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(account);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id),
            new Claim(ClaimTypes.Name, account.UserName!),
            new Claim(ClaimTypes.Email, account.Email!)
        };

        // Отримуємо всі клейми, які має користувач (якщо вони є)
        var userClaims = await _userManager.GetClaimsAsync(account);
        claims.AddRange(userClaims);

        // Додаємо ролі в токен
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role)); // 🔹 Правильний спосіб для Identity
        }
        
        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(_expiresInHours),
            signingCredentials: secretKey,
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}