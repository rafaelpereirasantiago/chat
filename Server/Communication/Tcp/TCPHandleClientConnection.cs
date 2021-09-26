using Client.Models.Communication;
using Server.Interfaces.Communication;
using Server.Observers;
using Server.Models.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Communication.Tcp
{
    public class TCPHandleClientConnection : Observable, IHandleClientConnection
    {
        private bool _active;
        private ProtocolConnection _protocol = ProtocolConnection.TCP;

        bool IHandleClientConnection.Active => _active;

        ProtocolConnection IHandleClientConnection.Protocol => _protocol;

        ~TCPHandleClientConnection()
        {
            _active = false;
        }

        void IHandleClientConnection.Close()
        {
            _active = false;
        }

        void IHandleClientConnection.Start(ListnerConnectionParameters parameters)
        {
            IPAddress localAdd = IPAddress.Parse(parameters.IP);
            TcpListener listener = new TcpListener(localAdd, parameters.Port);
            listener.Start();

            _active = true;
            Thread _tcpListenerThread = new Thread(() =>
            {
                while (_active)
                {
                    try
                    {
                        ReadMessage(listener);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        private bool IsRegisterUserMessage(string message)
        {
            return message.StartsWith("/register-user ");
        }

        private void NotifyNewUser(TcpClient clientSocket, string message)
        {
            string[] parameters = message.Split(" ");

            if (parameters.Length >= 4)
            {
                string nickName = parameters[1];

                var handleClienteMessage = new HandleClientMessage()
                {
                    Protocol = ProtocolConnection.TCP,
                    ConnectionObject = clientSocket,
                    User = new User()
                    {
                        NickName = nickName
                    }
                };

                notifyObservers(handleClienteMessage);
            }
        }

        void ReadMessage(TcpListener listener)
        {
            TcpClient clientSocket = listener.AcceptTcpClient();
            NetworkStream networkStream = clientSocket.GetStream();
            byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = networkStream.Read(buffer, 0, clientSocket.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (IsRegisterUserMessage(dataReceived))
            {
                NotifyNewUser(clientSocket, dataReceived);
            }
        }

    }
}
