using Auction.BL.Model.Account.DTO;

namespace Auction.BL.Interface;

public interface IAccountService
{
    public Task<string?> Login(AccountLoginDTO accountDto);
    public Task Register(AccountRegistrationDTO accountDto, uint role);
    public Task AddRoleToAccount(string id, string role);
    public Task RemoveRoleToAccount(string id, string role);

    //public Task AddRole(string role);
}