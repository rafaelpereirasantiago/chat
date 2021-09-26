using Client;
using Client.Communication;
using Client.Interfaces;
using Client.Models.Communication;
using NUnit.Framework;
using Server.Communication;
using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.Linq;
using System.Threading;

namespace ServerTests
{

    public class Tests
    {
        const int DEFAULT_TCP_PORT = 8002;
        const int DEFAULT_HTTP_PORT = 8003;
        const int DEFAULT_WEBSOCKET_PORT = 8004;

        const int DEFAULT_CALLBACK_PORT_TCP_CLIENT = 8005;
        const int DEFAULT_CALLBACK_PORT_HTTP_CLIENT = 8006;
        const int DEFAULT_CALLBACK_PORT_WEBSOCKET_CLIENT = 8007;

        private IChat _tcpChat;
        private IChat _httpChat;
        private IChat _websocketChat;
        private IServer _server;

        [OneTimeSetUp]
        public void Setup()
        {
            _tcpChat = CreateClient(Client.Communication.ProtocolConnection.TCP);
            _httpChat = CreateClient(Client.Communication.ProtocolConnection.HTTP);
            _websocketChat = CreateClient(Client.Communication.ProtocolConnection.WebSocket);
            
            _server = CreateServer();

            _server.Initialize();

            _tcpChat.Initialize();
            _httpChat.Initialize();
            _websocketChat.Initialize();
        }

        [Test, Order(1)]
        public void VerifyServices()
        {
            Assert.AreEqual(true, _server.Active);
            Assert.AreEqual(true, _tcpChat.Connected);
            Assert.AreEqual(true, _httpChat.Connected);
            Assert.AreEqual(true, _websocketChat.Connected);
        }

        [Test, Order(2)]
        public void RegisterUser()
        {
            var user1 = new Client.Models.User()
            {
                IP = "127.0.0.1",
                CallbackPort = DEFAULT_CALLBACK_PORT_TCP_CLIENT,
                ClientID = Guid.NewGuid().ToString(),
                NickName = "user1"
            };

            var user2 = new Client.Models.User()
            {
                IP = "127.0.0.1",
                CallbackPort = DEFAULT_CALLBACK_PORT_HTTP_CLIENT,
                ClientID = Guid.NewGuid().ToString(),
                NickName = "user2"
            };

            var user3 = new Client.Models.User()
            {
                IP = "127.0.0.1",
                CallbackPort = DEFAULT_CALLBACK_PORT_WEBSOCKET_CLIENT,
                ClientID = Guid.NewGuid().ToString(),
                NickName = "user3"
            };

            var response1 = _tcpChat.RegisterUser(user1);
            var response2 = _httpChat.RegisterUser(user2);
            var response3 = _tcpChat.RegisterUser(user3);

            Assert.AreEqual(TypeSendCommandResponse.Successful, response1.Type);
            Assert.AreEqual(TypeSendCommandResponse.Successful, response2.Type);
            Assert.AreEqual(TypeSendCommandResponse.Successful, response3.Type);

            Assert.AreEqual(true,_server.ConnectedUsers.Any(user => user.User.NickName == user1.NickName));
            Assert.AreEqual(true, _server.ConnectedUsers.Any(user => user.User.NickName == user2.NickName));
            Assert.AreEqual(true, _server.ConnectedUsers.Any(user => user.User.NickName == user3.NickName));
        }

        [Test,Order(3)]
        public void ListenServer()
        {
            _tcpChat.ListenServer();
            _httpChat.ListenServer();
            _websocketChat.ListenServer();
        }


        private IServer CreateServer()
        {
            IGatewayHandleClientConnection getwayListnerConnection = new GatewayHandleClientConnection(
                new GatewayListnerConnectionParameters()
                {
                    IP = "127.0.0.1",
                    HttpPort = DEFAULT_HTTP_PORT,
                    TcpPort = DEFAULT_TCP_PORT,
                    WebsocketPort = DEFAULT_WEBSOCKET_PORT
                }
            );

            return new Server.Server(
                new Server.CommandInterpreter(),
                getwayListnerConnection
            ); 
        }

        private IChat CreateClient(Client.Communication.ProtocolConnection protocol)
        {
            string clientId = Guid.NewGuid().ToString();
            ServerConnectionParameters serverConnectionParameters = new ServerConnectionParameters()
            {
                ClientID = clientId,
                LocalIP = "127.0.0.1",
                IP = "127.0.0.1",
                Port = GetPortByProtocol(protocol),
                CallbackPort = GetPortByProtocol(protocol)
            };

            var serverConnection = ServerConnectionFactory.Create(protocol);

            return new Chat(
                serverConnection,
                serverConnectionParameters
            );
        }

        private int GetPortByProtocol(Client.Communication.ProtocolConnection protocol)
        {
            var port = DEFAULT_TCP_PORT;
            if (protocol == Client.Communication.ProtocolConnection.HTTP) port = DEFAULT_HTTP_PORT;
            if (protocol == Client.Communication.ProtocolConnection.WebSocket) port = DEFAULT_WEBSOCKET_PORT;

            return port;
        }

        private int GetCallbackPortByProtocol(Client.Communication.ProtocolConnection protocol)
        {
            var port = DEFAULT_CALLBACK_PORT_TCP_CLIENT;
            if (protocol == Client.Communication.ProtocolConnection.HTTP) port = DEFAULT_CALLBACK_PORT_HTTP_CLIENT;
            if (protocol == Client.Communication.ProtocolConnection.WebSocket) port = DEFAULT_CALLBACK_PORT_WEBSOCKET_CLIENT;

            return port;
        }
    }
}