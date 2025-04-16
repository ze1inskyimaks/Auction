using Auction.BL.Model.AuctionLot;

namespace Auction.BL.Model.Mapping;

public static class AuctionLotMapping
{
    public static Data.Model.AuctionLot ToAuctionLot(AuctionLotDtoInput lotInput, Data.Model.Account ownerAccount)
    {
        var lot = new Data.Model.AuctionLot()
        {
            Id = Guid.Empty,
            Name = lotInput.Name,
            Description = lotInput.Description,
            StartTime = lotInput.StartTime,
            OwnerId = Guid.Parse(ownerAccount.Id),
            OwnerAccount = ownerAccount,
            StartPrice = lotInput.StartPrice,
        };
        return lot;
    }

    public static AuctionLotDtoOutput ToDto(Data.Model.AuctionLot lot)
    {
        var dto = new AuctionLotDtoOutput
        {
            Id = lot.Id,
            Name = lot.Name,
            Description = lot.Description,
            StartTime = lot.StartTime,
            OwnerId = lot.OwnerId,
            CurrentWinnerId = lot.CurrentWinnerId,
            CurrentPrice = lot.CurrentPrice,
            LastBitTime = lot.LastBitTime,
            AuctionHistoryId = lot.AuctionHistoryId,
            StartPrice = lot.StartPrice,
            EndPrice = lot.EndPrice,
            WinnerId = lot.WinnerId,
            Status = lot.Status,
            UpdatedAt = lot.UpdatedAt,
            CreatedAt = lot.CreatedAt
        };
        return dto;
    }
}