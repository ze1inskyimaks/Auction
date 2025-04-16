namespace Auction.BL.Model.Account.DTO;

public class AccountDTO
{
    public long? Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; }  = null!;
}