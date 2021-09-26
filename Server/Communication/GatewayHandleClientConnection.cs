using Client.Models.Communication;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Communication
{
    public class GatewayHandleClientConnection : IGatewayHandleClientConnection
    {
        private GatewayListnerConnectionParameters _gatewayListnerConnectionParameters;
        private IHandleClientConnection _listnerWebsocketConnection;
        private IHandleClientConnection _listnerHttpConnection;
        private IHandleClientConnection _listnerTcpConnection;
        private List<IHandleClientConnection> _connections = new List<IHandleClientConnection>();
        private Dictionary<ProtocolConnection, int> _protocolPorts = new Dictionary<ProtocolConnection, int>();

        public GatewayHandleClientConnection(GatewayListnerConnectionParameters gatewayListnerConnectionParameters)
        {
            _gatewayListnerConnectionParameters = gatewayListnerConnectionParameters;
            _listnerWebsocketConnection = HandleClientConnectionFactory.Create(ProtocolConnection.WebSocket);
            _listnerTcpConnection = HandleClientConnectionFactory.Create(ProtocolConnection.TCP);
            _listnerHttpConnection = HandleClientConnectionFactory.Create(ProtocolConnection.HTTP);
            AddConnection(_listnerWebsocketConnection);
            AddConnection(_listnerTcpConnection);
            AddConnection(_listnerHttpConnection);

            _protocolPorts.Add(ProtocolConnection.WebSocket, _gatewayListnerConnectionParameters.WebsocketPort);
            _protocolPorts.Add(ProtocolConnection.HTTP, _gatewayListnerConnectionParameters.HttpPort);
            _protocolPorts.Add(ProtocolConnection.TCP, _gatewayListnerConnectionParameters.TcpPort);
        }

        IHandleClientConnection IGatewayHandleClientConnection.TcpConnection => _listnerTcpConnection;

        IHandleClientConnection IGatewayHandleClientConnection.HttpConnection => _listnerHttpConnection;

        IHandleClientConnection IGatewayHandleClientConnection.WebsocketConnection => _listnerWebsocketConnection;

        bool IGatewayHandleClientConnection.Active => allConnectionActive();

        private bool allConnectionActive()
        {
            foreach (var connection in _connections)
            {
                if (!connection.Active) return false;
            }
            return true;
        }

        void IGatewayHandleClientConnection.Start()
        {
            _connections.ForEach(connection => {
                var port = PortByProtocol(connection.Protocol);
                var parameters = new ListnerConnectionParameters(
                    _gatewayListnerConnectionParameters.IP,
                    port
                );
                connection.Start(parameters);       
            });
        }

        void IGatewayHandleClientConnection.Stop()
        {
            _connections.ForEach(connection => connection.Close());
        }

        void IGatewayHandleClientConnection.Subscribe(IObserver<HandleClientMessage> observer)
        {
            _connections.ForEach(connection => connection.Subscribe(observer));
        }

        private int PortByProtocol(ProtocolConnection protocol)
        {
            return _protocolPorts.Where(item => item.Key == protocol).First().Value;
        }

        private void AddConnection(IHandleClientConnection connection)
        {
            if (connection != null) _connections.Add(connection);
        }
    }
}
