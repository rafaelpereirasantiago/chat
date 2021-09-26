using Client.Communication.Http;
using Client.Communication.Tcp;
using Client.Communication.Websocket;
using Client.Interfaces.Communication;
using System;
using System.Collections.Generic;

namespace Client.Communication
{
    public class ServerConnectionFactory
    {
        public static IServerConnection Create(ProtocolConnection type)
        {
            var commandList = ServerConnectionFactory.Definitions();

            foreach (var item in commandList)
            {
                if (item.Key(type))
                {
                    return item.Value();
                }
            }

            throw new Exception("Invalid type connection");
        }

        private static Dictionary<Func<ProtocolConnection, bool>, Func<IServerConnection>> Definitions()
        {
            var commandList = new Dictionary<Func<ProtocolConnection, bool>, Func<IServerConnection>>();
            
            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.TCP, () => new TCPServerConnection());
            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.HTTP, () => new HttpServerConnection());
            commandList.Add((ProtocolConnection type) => type == ProtocolConnection.WebSocket, () => new WebsocketServerConnection());

            return commandList;
        }
    }
}
