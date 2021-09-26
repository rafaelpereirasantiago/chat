using Client.Interfaces;
using Client.Interfaces.Communication;
using Client.Models.Communication;
using Client.Models;
using Client.Observers;

namespace Client
{
    public class Chat: IChat
    {
        private IServerConnection _connection;
        private User _user;
        private string selectedRoom = "#general";
        private ServerConnectionParameters _connectionParameters;

        bool IChat.Connected => _connection.Connected;

        string IChat.SelectedRoom => selectedRoom;

        public Chat(
            IServerConnection connection,
            ServerConnectionParameters connectionParameters
        )
        {
            _connection = connection; 
            _connectionParameters = connectionParameters;
        }

        void IChat.Initialize()
        {
            //_callBackConnection.Start(_callbackConnectionParameters);
            _connection.Connect(_connectionParameters);
            var serverConnectionObserver = new ServerConnectionObserver();
            _connection.Subscribe(serverConnectionObserver);
        }

        SendCommandResponse IChat.RegisterUser(User user)
        {
            user.ClientID = _connectionParameters.ClientID;
            var serverResponse = _connection.SendCommandWithCallback($"/register-user {user.NickName} {user.IP} {user.Port} {user.ClientID} {user.CallbackPort}");
            if (serverResponse.Type == TypeSendCommandResponse.Successful && serverResponse.ResponseMessage == "ok")
            {
                _user = user;
            }

            return serverResponse;
        }

        SendCommandResponse IRoomManager.CreateRoom(Room room)
        {
            var serverResponse = _connection.SendCommand($"/create-room {_user.NickName} {room.Name}");
            if (serverResponse.Type == TypeSendCommandResponse.Successful)
            {
                selectedRoom = room.Name;
            }
            return serverResponse;
        }

        SendCommandResponse IRoomManager.SelectRoom(string chatRoom)
        {
            var serverResponse = _connection.SendCommand($"/select-room {_user.NickName} {chatRoom}");
            if (serverResponse.Type == TypeSendCommandResponse.Successful)
            {
                selectedRoom = chatRoom;
            }
            return serverResponse;
        }

        SendCommandResponse IMessageSender.SendPublicMessage(string message)
        {
            return _connection.SendCommand($"/m {_user.NickName} {selectedRoom} {message}");
        }

        SendCommandResponse IMessageSender.SendPublicMessageForUser(string user, string message)
        {
            return _connection.SendCommand($"/p {_user.NickName} {selectedRoom} {user} {message}");
        }

        SendCommandResponse IChat.Quit()
        {
            _connection.StopListen();
            var serverResponse = _connection.SendCommand($"/exit {_user.NickName}");
            if (serverResponse.Type == TypeSendCommandResponse.Successful)
            {
                _connection.Disconnect();
            }
            return serverResponse;
        }

        void IChat.ListenServer()
        {
            _connection.Listen();
        }
    }
}
