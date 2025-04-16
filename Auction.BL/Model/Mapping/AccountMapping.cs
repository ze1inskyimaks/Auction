using Auction.BL.Model.Account.DTO;

namespace Auction.BL.Model.Mapping;

public static class AccountMapping
{
    public static Data.Model.Account ToModel(AccountDTO accountDto)
    {
        var model = new Data.Model.Account()
        {
            Email = accountDto.Email,
            UserName = accountDto.UserName
        };

        return model;
    }

    public static AccountDTO ToDto(Data.Model.Account account)
    {
        var dto = new AccountDTO()
        {
            Id = long.TryParse(account.Id, out var id) ? id : null,
            Email = account.Email!,
            UserName = account.UserName!
        };

        return dto;
    }
}