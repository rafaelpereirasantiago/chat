using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Communication.Tcp
{
    public class TCPUserConnection : IUserConnection
    {
        private ICommandInterpreter _commandInterpreter;
        private User _user;
        private TcpClient _socket;
        private bool _active = false;

        User IUserConnection.User { get => _user; }
        ProtocolConnection IUserConnection.Protocol { get => ProtocolConnection.TCP; }

        public TCPUserConnection(
            ICommandInterpreter commandInterpreter,
            object connectionObject,
            User user)
        {
            _user = user;
            _socket = (TcpClient)connectionObject;
            _commandInterpreter = commandInterpreter;
        }

        void IUserConnection.Listen()
        {
            _active = true;
            Thread _tcpListenerThread = new Thread(() =>
            {
                while (_active)
                {
                    try
                    {
                        ReadMessage();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Sutdown();
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        private void ReadMessage()
        {
            var stream = _socket.GetStream();
            byte[] buffer = new byte[_socket.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, _socket.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            _commandInterpreter.Interpret(dataReceived);
        }

        public void SendMessage(string message)
        {
            var stream = _socket.GetStream();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
            stream.Write(bytesToSend);
            stream.Flush();
        }

        public void Sutdown()
        {
            _active = false;
        }

        void IUserConnection.NotifyUserAlreadyExists()
        {
            SendMessage("user-already-exists");
        }

        void IUserConnection.NotifyRegisteredUser()
        {
            SendMessage("ok");
        }
    }
}
