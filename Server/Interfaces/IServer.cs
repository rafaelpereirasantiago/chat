using Server.Communication;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server.Interfaces
{
    public interface IServer: IMessageReceiver, IRoomManager
    {
        bool Active { get; }
        IEnumerable<IUserConnection> ConnectedUsers { get; }
        void Initialize();
        void Close();
        ReceiveCommandResponse RegisterUser(ProtocolConnection protocol, object connectionObject, User user);
        ReceiveCommandResponse Quit(string user);
    }
}
