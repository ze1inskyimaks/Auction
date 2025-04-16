namespace Auction.BL.Interface;

public interface ILobbyManagerService
{
    public Task CreateLobby();
    public Task GetLobbyById(Guid id);
}