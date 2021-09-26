
using Server.Models.Communication;
using System.Collections.Generic;

namespace Server.Interfaces.Communication
{
    public interface IRoomManager
    {
        IEnumerable<Room> Rooms { get; }
        ReceiveCommandResponse CreateRoom(Room room);

        ReceiveCommandResponse AddUserToRoom(string user, string chatRoom);

        ReceiveCommandResponse RemoveUserToRoom(string user, string chatRoom);
    }
}
