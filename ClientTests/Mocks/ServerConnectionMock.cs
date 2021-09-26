using Client.Interfaces.Communication;
using Client.Models.Communication;
using Client.Observers;

namespace ClientTests.Mocks
{
    class ServerConnectionMock : Observable, IServerConnection
    {
        private bool _connected = false;
        private bool _listen = false;
        bool IServerConnection.Connected => _connected;

        public bool isListenServer => _listen;

        ConnectionResponse IServerConnection.Connect(ServerConnectionParameters parameters)
        {
            _connected = true;
            return new ConnectionResponse()
            {
                Status = StatusConnection.Successful
            };
        }

        void IServerConnection.Disconnect()
        {
            _connected = false;
        }

        SendCommandResponse IServerConnection.SendCommandWithCallback(string command)
        {
            string responseMessage = "";

            if (command.StartsWith("/register-user wrongUser"))
            {
                responseMessage = "user-already-exists";
            }
            else if (command.StartsWith("/register-user "))
            {
                responseMessage = "ok";
            }

            return new SendCommandResponse()
            {
                Command = command,
                Type = TypeSendCommandResponse.Successful,
                ResponseMessage = responseMessage
            };
        }

        SendCommandResponse IServerConnection.SendCommand(string command)
        {
            return new SendCommandResponse() { 
                Command  = command,
                Type = TypeSendCommandResponse.Successful
            };
        }

        void IServerConnection.Listen()
        {
            _listen = true;
        }

        void IServerConnection.StopListen()
        {
            _listen = false;
        }
    }
}
