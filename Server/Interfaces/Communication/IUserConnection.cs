using Server.Communication;
using Server.Models.Communication;

namespace Server.Interfaces.Communication
{
    public interface IUserConnection
    {
        ProtocolConnection Protocol { get; }
        User User { get; }
        void Listen();
        void Sutdown();
        void SendMessage(string message);
        void NotifyUserAlreadyExists();
        void NotifyRegisteredUser();
    }
}
