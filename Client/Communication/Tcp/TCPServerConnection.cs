using Client.Interfaces.Communication;
using Client.Observers;
using Client.Models.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client.Communication.Tcp
{
    public class TCPServerConnection: Observable, IServerConnection
    {
        private TcpClient _tcpClient = new TcpClient();
        private bool _listen = false;

        bool IServerConnection.Connected => _tcpClient.Connected;

        ConnectionResponse IServerConnection.Connect(ServerConnectionParameters parameters)
        {
            IPAddress ipAddress = IPAddress.Parse(parameters.IP);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, parameters.Port);
            try
            {
                _tcpClient.Connect(ipEndPoint);
                return new ConnectionResponse();
            }
            catch (Exception ex)
            {
                return new ConnectionResponse()
                {
                    Status = StatusConnection.Error,
                    MessageError = ex.Message
                };
            }
        }

        void IServerConnection.Disconnect()
        {
            if (_tcpClient.Connected)
                _tcpClient.Close();
        }

        private void WriteStream(string data)
        {
            var networkStream = _tcpClient.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(data);
            networkStream.Write(bytesToSend);
            networkStream.Flush();
        }

        private string ReadStream()
        {
            var networkStream = _tcpClient.GetStream();
            byte[] receiveBuffer = new byte[1024];

            int bytesRead = networkStream.Read(receiveBuffer, 0, receiveBuffer.Length);
            // Decode the data.
            return Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead);
        }

        SendCommandResponse IServerConnection.SendCommand(string command)
        {
            WriteStream(command);
            return new SendCommandResponse()
            {
                Type = TypeSendCommandResponse.Successful,
                Command = command
            };
        }

        SendCommandResponse IServerConnection.SendCommandWithCallback(string command)
        {
            WriteStream(command);
            string responseMessage = ReadStream();

            return new SendCommandResponse()
            {
                Type = TypeSendCommandResponse.Successful,
                Command = command,
                ResponseMessage = responseMessage
            };
        }

        private void ListenServer()
        {
            string data = _listen ? ReadStream() : "";
            notifyObservers(data);
        }

        void IServerConnection.Listen()
        {
            Thread _tcpListenerThread = new Thread(() =>
            {
                _listen = true;
                while (_listen && _tcpClient.Connected)
                {
                    try
                    {
                        ListenServer();
                    }
                    catch (Exception)
                    {
                        _listen = false;
                        //Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        void IServerConnection.StopListen()
        {
            _listen = false;
        }
    }
}
