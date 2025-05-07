using Auction.BL.Model.Account.DTO;

namespace Auction.BL.Model.Mapping;

public static class AccountMapping
{
    public static Data.Model.Account ToModelRegistration(AccountRegistrationDTO accountDto)
    {
        var model = new Data.Model.Account()
        {
            Email = accountDto.Email,
            UserName = accountDto.UserName
        };

        return model;
    }

    public static AccountRegistrationDTO ToDtoRegistration(Data.Model.Account account)
    {
        var dto = new AccountRegistrationDTO()
        {
            Id = long.TryParse(account.Id, out var id) ? id : null,
            Email = account.Email!,
            UserName = account.UserName!
        };

        return dto;
    }
    
    public static Data.Model.Account ToModelLogin(AccountLoginDTO accountDto)
    {
        var model = new Data.Model.Account()
        {
            Email = accountDto.Email,
        };

        return model;
    }

    public static AccountLoginDTO ToDtoLogin(Data.Model.Account account)
    {
        var dto = new AccountLoginDTO()
        {
            Email = account.Email!,
        };

        return dto;
    }
}