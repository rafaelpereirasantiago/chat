using Server.Models.Communication;

namespace Server.Interfaces.Communication
{
    public interface IMessageReceiver
    {
        ReceiveCommandResponse ReceivePublicMessageForUser(string senderUser, string room, string user, string message);
        ReceiveCommandResponse ReceivePublicMessage(string senderUser, string room, string message);
    }
}
