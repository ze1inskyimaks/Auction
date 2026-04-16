using System.Security.Claims;
using Auction.BL.Interface;
using Auction.BL.Model.Account.DTO;
using Auction.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API;

[ApiController]
[Route("api/v1/identity")]
public class IdentityApi : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly UserManager<Account> _userManager;

    public IdentityApi(IAccountService accountService, UserManager<Account> userManager)
    {
        _accountService = accountService;
        _userManager = userManager;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody]AccountLoginDTO accountDto)
    {
        try
        {
            var token = await _accountService.Login(accountDto);

            if (token == null)
            {
                return Unauthorized(new { message = "Невірний email або пароль." });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Забороняє доступ через JavaScript
                Secure = true, // Включити у HTTPS
                Expires = DateTime.UtcNow.AddHours(12)
            };
            Response.Cookies.Append("boby", token, cookieOptions);
        
            return Ok(new { token, message = "Logged in successfully" });
        }
        catch (Exception e)
        {
            return MapIdentityError(e, "Помилка авторизації.");
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody]AccountRegistrationDTO accountDto, uint role = default)
    {
        try
        {
            await _accountService.Register(accountDto, role);
            return Ok();
        }
        catch (Exception e)
        {
            return MapIdentityError(e, "Помилка реєстрації.");
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("add_role")]
    public async Task<IActionResult> SetRoleForUser(string id, string role)
    {
        await _accountService.AddRoleToAccount(id, role);
        return Ok();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("remove_role")]
    public async Task<IActionResult> RemoveRoleForUser(string id, string role)
    {
        await _accountService.RemoveRoleToAccount(id, role);
        return Ok();
    }
    
    [Authorize(Roles = "USER")]
    [HttpGet("userinfouser")]
    public IActionResult TakeSecretInfoUser()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        if (!roles.Contains("USER"))
        {
            return Unauthorized();
        }

        return Ok("Secret info for User");
    }
    
    [Authorize(Roles = "ADMIN")] //працю лише після оновлення jwt токена користувачем
    [HttpGet("userinfoadmin")]
    public IActionResult TakeSecretInfoAdmin()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        if (!roles.Contains("ADMIN"))
        {
            return Unauthorized();
        }

        return Ok("Secret info for Admin");
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin/user-profile")]
    public async Task<IActionResult> GetUserProfileForAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "User id is required." });
        }

        var account = await _userManager.FindByIdAsync(id);
        if (account == null)
        {
            return NotFound(new { message = "Користувача не знайдено." });
        }

        var roles = await _userManager.GetRolesAsync(account);

        return Ok(new
        {
            account.Id,
            account.UserName,
            account.Email,
            account.PhoneNumber,
            Roles = roles
        });
    }

    private IActionResult MapIdentityError(Exception exception, string fallbackMessage)
    {
        var message = exception.Message;

        if (message.Contains("вже існує", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("не вдалося створити користувача", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("User indefinite", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Wrong password", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedMessage = message.Contains("User indefinite", StringComparison.OrdinalIgnoreCase) ||
                                    message.Contains("Wrong password", StringComparison.OrdinalIgnoreCase)
                ? "Невірний email або пароль."
                : message;

            return BadRequest(new { message = normalizedMessage });
        }

        return Problem(
            detail: $"{fallbackMessage} {message}",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
