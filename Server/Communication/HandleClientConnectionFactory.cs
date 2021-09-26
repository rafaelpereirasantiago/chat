using Server.Communication.Http;
using Server.Communication.Tcp;
using Server.Communication.Websocket;
using Server.Interfaces.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Communication
{
    class HandleClientConnectionFactory
    {
        public static IHandleClientConnection Create(ProtocolConnection type)
        {
            var commandList = HandleClientConnectionFactory.Definitions();

            foreach (var item in commandList)
            {
                if (item.Key(type))
                {
                    return item.Value();
                }
            }

            throw new Exception("Invalid type connection");
        }

        private static Dictionary<Func<ProtocolConnection, bool>, Func<IHandleClientConnection>> Definitions()
        {
            var commandList = new Dictionary<Func<ProtocolConnection, bool>, Func<IHandleClientConnection>>();

            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.TCP, () => new TCPHandleClientConnection());
            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.HTTP, () => new HttpHandleClientConnection());
            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.WebSocket, () => new WebsocketHandleClientConnection());

            return commandList;
        }
    }
}
