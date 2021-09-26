using Client.Models.Communication;
using Client.Models;

namespace Client.Interfaces.Communication
{
    public interface IRoomManager
    {
        SendCommandResponse CreateRoom(Room room);

        SendCommandResponse SelectRoom(string chatRoom);
    }
}
