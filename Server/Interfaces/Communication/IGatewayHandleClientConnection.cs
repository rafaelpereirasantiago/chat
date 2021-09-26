using Server.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Interfaces.Communication
{
    public interface IGatewayHandleClientConnection
    {
        bool Active { get; }
        IHandleClientConnection TcpConnection { get; }
        IHandleClientConnection HttpConnection { get; }
        IHandleClientConnection WebsocketConnection { get; }

        void Start();
        void Stop();

        void Subscribe(IObserver<HandleClientMessage> observable);
    }
}
