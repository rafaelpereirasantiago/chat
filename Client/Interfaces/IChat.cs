using Client.Interfaces.Communication;
using Client.Models.Communication;
using Client.Models;

namespace Client.Interfaces
{
    public interface IChat: IMessageSender, IRoomManager
    {
        bool Connected { get; }
        bool isListenServer { get; }
        string SelectedRoom { get; }
        void Initialize();
        void ListenServer();
        SendCommandResponse RegisterUser(User user);
        SendCommandResponse Quit();
    }
}
