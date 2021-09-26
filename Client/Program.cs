using Client.Communication;
using Client.Interfaces;
using Client.Models;
using Client.Models.Communication;
using System;

namespace Client
{
    class Program
    {
        const string LOCALHOST = "127.0.0.1";
        const int DEFAULT_TCP_PORT = 8002;
        const int DEFAULT_HTTP_PORT = 8003;
        const int DEFAULT_WEBSOCKET_PORT = 8004;
        const int DEFAULT_CALLBACK_PORT = 8005;

        static void Main(string[] args)
        {
            var protocol = getProtocolByArgs(args);
            var serverAddress = getServerAddressByArgs(args);
            var port = getPortByArgs(args,protocol);
            var callbackPort = getCallbackPortByArgs(args, protocol);

            IChat chat = CreateChatObject(
                protocol,
                port,
                callbackPort,
                serverAddress
            );

            chat.Initialize();

            if (chat.Connected)
            {
                ICommandInterpreter interpreter = new CommandInterpreter(chat);

                SendUser(chat, port, callbackPort);
                chat.ListenServer();

                while (chat.Connected)
                {
                    var command = Console.ReadLine();
                    interpreter.Interpret(command);
                }
            }
            else
            {
                Console.WriteLine("Unable to connect server");
            }

            Console.WriteLine("Disconnected...");
        }

        static void SendUser(IChat chat, int port, int callbackPort)
        {
            SendCommandResponse registerUserRespone = null;

            Console.WriteLine("Welcome to your chat server.");
            Console.WriteLine("Please provider a nickname:");
            var nickName = Console.ReadLine();

            while (registerUserRespone == null || registerUserRespone.ResponseMessage != "ok")
            {
                registerUserRespone = chat.RegisterUser(new User()
                {
                    IP = LOCALHOST,
                    Port = port,
                    NickName = nickName,
                    CallbackPort = callbackPort
                });

                if (registerUserRespone.ResponseMessage != "ok")
                {
                    Console.WriteLine($"Error: {registerUserRespone.ResponseMessage}.");
                    Console.WriteLine("Please provider a nickname:");
                    nickName = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Nickname accepted");
                    PrintHelp();
                }
            }
        }
        static IChat CreateChatObject(ProtocolConnection protocol, int port, int callbackPort, string serverAddress)
        {
            string clientId = Guid.NewGuid().ToString();
            ServerConnectionParameters serverConnectionParameters = new ServerConnectionParameters()
            {
                ClientID = clientId,
                LocalIP = LOCALHOST,
                IP = serverAddress,
                Port = port,
                CallbackPort = callbackPort
            };

            var serverConnection = ServerConnectionFactory.Create(protocol);

            return new Chat(
                serverConnection,
                serverConnectionParameters
            );
        }

        static string findArgument(string[] args, string initial)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith(initial)) return arg;
            }
            return "";
        }

        static ProtocolConnection getProtocolByArgs(string[] args)
        {
            var protocol = ProtocolConnection.TCP;
            var prefix = "-protocol=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                if (arg == "http") return ProtocolConnection.HTTP;
                if (arg == "websocket") return ProtocolConnection.WebSocket;
            }

            return protocol;
        }

        static string getServerAddressByArgs(string[] args)
        {
            var serverAddress = LOCALHOST;
            var prefix = "-server=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "") serverAddress = arg;

            return serverAddress;
        }

        static int getPortByArgs(string[] args, ProtocolConnection protocol)
        {
            var port = DEFAULT_TCP_PORT;
            if (protocol == ProtocolConnection.HTTP) port = DEFAULT_HTTP_PORT;
            if (protocol == ProtocolConnection.WebSocket) port = DEFAULT_WEBSOCKET_PORT;

            var prefix = "-port=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                int newPort;
                if (int.TryParse(arg, out newPort)) port = newPort;
            }

            return port;
        }

        static int getCallbackPortByArgs(string[] args, ProtocolConnection protocol)
        {
            var port = DEFAULT_CALLBACK_PORT;
            var prefix = "-callbackPort=";
            string arg = findArgument(args, prefix).Replace(prefix, "");
            if (arg != "")
            {
                int newPort;
                if (int.TryParse(arg, out newPort)) port = newPort;
            }

            return port;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Commands reference:");
            Console.WriteLine("  A public message to everyone in the room: /m [message]");
            Console.WriteLine("  A public message to a room user: /p [user] [message]");
            Console.WriteLine("  Exit chat: /exit");
        }
    }
}
