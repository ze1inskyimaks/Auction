namespace Auction.BL.Model.Account.DTO;

public class AccountRegistrationDTO
{
    public long? Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; }  = null!;
}