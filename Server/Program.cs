using Server.Communication;
using Server.Interfaces;
using Server.Models.Communication;
using System;
using Server.Interfaces.Communication;

namespace Server
{
    
    class Program
    {
        const int DEFAULT_TCP_PORT = 8002;
        const int DEFAULT_HTTP_PORT = 8003;
        const int DEFAULT_WEBSOCKET_PORT = 8004;

        const string LOCALHOST = "127.0.0.1";
        static void Main(string[] args)
        {
            int tcpPort = getTcpPortByArgs(args);
            int httpPort = getHttpPortByArgs(args);
            int webSocketPort = getWebsocketPortByArgs(args);

            IGatewayHandleClientConnection getwayListnerConnection = new GatewayHandleClientConnection(
                new GatewayListnerConnectionParameters()
                {
                    IP = LOCALHOST,
                    HttpPort = httpPort,
                    TcpPort = tcpPort,
                    WebsocketPort = webSocketPort
                }
            );

            IServer _server = new Server(
                new CommandInterpreter(),
                getwayListnerConnection
            );
            _server.Initialize();

            Console.WriteLine("Server started!");
        }

        static string findArgument(string[] args, string initial)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith(initial)) return arg;
            }
            return "";
        }

        static int getTcpPortByArgs(string[] args)
        {
            var port = DEFAULT_TCP_PORT;

            var prefix = "-tcpPort=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                int newPort;
                if (int.TryParse(arg, out newPort)) port = newPort;
            }

            return port;
        }

        static int getHttpPortByArgs(string[] args)
        {
            var port = DEFAULT_HTTP_PORT;

            var prefix = "-httpPort=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                int newPort;
                if (int.TryParse(arg, out newPort)) port = newPort;
            }

            return port;
        }

        static int getWebsocketPortByArgs(string[] args)
        {
            var port = DEFAULT_WEBSOCKET_PORT;

            var prefix = "-websocketPort=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                int newPort;
                if (int.TryParse(arg, out newPort)) port = newPort;
            }

            return port;
        }
    }
}
