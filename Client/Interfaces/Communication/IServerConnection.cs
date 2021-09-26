using Client.Communication;
using Client.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Interfaces.Communication
{
    public interface IServerConnection: IObservable<string>
    {
        bool Connected { get; }
        bool isListenServer { get; }
        ConnectionResponse Connect(ServerConnectionParameters parameters);
        SendCommandResponse SendCommand(string command);
        SendCommandResponse SendCommandWithCallback(string command);

        void Listen();
        void StopListen();

        void Disconnect();
    }
}
