using Server.Communication;
using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using Server.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Server
{
    public class Server : IServer
    {
        ICommandInterpreter _commandInterpreter;
        private IGatewayHandleClientConnection _getwayHandleClientConnection;
        private List<IUserConnection> _users = new List<IUserConnection>();
        private List<Room> _rooms = new List<Room>();
        public Server(
            ICommandInterpreter commandInterpreter,
            IGatewayHandleClientConnection getwayHandleClientConnection
        )
        {
            _commandInterpreter = commandInterpreter;
            _getwayHandleClientConnection = getwayHandleClientConnection;
            _commandInterpreter.Server = this;
        }

        bool IServer.Active => _getwayHandleClientConnection.Active;

        IEnumerable<IUserConnection> IServer.ConnectedUsers => _users;

        IEnumerable<Room> IRoomManager.Rooms => _rooms;

        public ReceiveCommandResponse AddUserToRoom(string user, string chatRoom)
        {
            throw new NotImplementedException();
        }
        public void Close()
        {
            throw new NotImplementedException();
        }

        public ReceiveCommandResponse CreateRoom(Room room)
        {
            throw new NotImplementedException();
        }

        void IServer.Initialize()
        {
            var handleClientConnectionObserver = new HandleClientConnectionObserver(this);
            _getwayHandleClientConnection.Subscribe(handleClientConnectionObserver);

            AddDefautltRoom();

            _getwayHandleClientConnection.Start();
        }

        public void ListenUsers()
        {
            throw new NotImplementedException();
        }

        public ReceiveCommandResponse Quit(string user, string message)
        {
            throw new NotImplementedException();
        }

        public ReceiveCommandResponse RemoveUserToRoom(string user, string chatRoom)
        {
            throw new NotImplementedException();
        }


        private void AddDefautltRoom()
        {
            _rooms.Add(new Room()
            {
                Name = "#general"
            });
        }

        private ReceiveCommandResponse GenerateErrorResponse(string message)
        {
            return new ReceiveCommandResponse()
            {
                Type = TypeReceiveCommandResponse.Error,
                MessageError = message
            };
        }

        private ReceiveCommandResponse GenerateSuccessfulResponse()
        {
            return new ReceiveCommandResponse()
            {
                Type = TypeReceiveCommandResponse.Successful
            };
        }

        void IServer.Close()
        {
            _users.ForEach(user => user.Sutdown());
            _getwayHandleClientConnection.Stop();
        }


        ReceiveCommandResponse IServer.RegisterUser(ProtocolConnection protocol, object connectionObject, User user)
        {
            
            IUserConnection userConnection = UserConnectionFactory.Create(
                protocol,
                _commandInterpreter,
                connectionObject,
                user
            );

            if (_users.Any(item => item.User.NickName.ToUpper() == user.NickName.ToUpper()))
            {
                userConnection.NotifyUserAlreadyExists();
                return GenerateErrorResponse("user-already-exists");
            }
            else
            {
                _users.Add(userConnection);
                _rooms.Where(item => item.Name == "#general").First().Users.Add(userConnection);

                if (protocol == ProtocolConnection.TCP) userConnection.Listen();

                userConnection.NotifyRegisteredUser();

                if (protocol != ProtocolConnection.TCP) userConnection.Listen();

                return GenerateSuccessfulResponse();
            }
        }

        ReceiveCommandResponse IServer.Quit(string user)
        {
            try
            {
                Predicate<IUserConnection> predicate = (item) => item.User.NickName.ToUpper() == user.ToUpper();

                _rooms.ForEach((room) => room.Users.RemoveAll(predicate));
                _users.RemoveAll(predicate);

                return new ReceiveCommandResponse();
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(ex.Message);
            }
        }

        ReceiveCommandResponse IMessageReceiver.ReceivePublicMessageForUser(string senderUser, string room, string user, string message)
        {
            string formattedMessage = $"{room}:{senderUser}: @{user}, {message}";
            return BroadCastToRoom(room, formattedMessage);
        }

        ReceiveCommandResponse IMessageReceiver.ReceivePublicMessage(string senderUser, string room, string message)
        {
            string formattedMessage = $"{room}:{senderUser}: {message}";
            return BroadCastToRoom(room, formattedMessage);
        }

        ReceiveCommandResponse IRoomManager.CreateRoom(Room room)
        {
            try
            {
                if (!_rooms.Any(room => room.Name.ToUpper().Equals(room.Name.ToUpper())))
                {
                    _rooms.Add(room);
                }
                else
                {
                    return GenerateErrorCommandResponse("room already exists");
                }

                return new ReceiveCommandResponse();
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(ex.Message);
            }
        }

        ReceiveCommandResponse IRoomManager.AddUserToRoom(string user, string chatRoom)
        {
            try
            {
                var room = _rooms.Where(room => room.Name.ToUpper().Equals(chatRoom.ToUpper())).FirstOrDefault();
                if (room != null)
                {
                    var selectedUser = _users.Where(item => item.User.NickName.ToUpper().Equals(user.ToUpper())).FirstOrDefault();
                    if (selectedUser != null)
                    {
                        if (room.Users.Any(item => item.User.NickName == selectedUser.User.NickName))
                            room.Users.Add(selectedUser);
                    }
                    else
                    {
                        return GenerateErrorCommandResponse("user does not exists in the room");
                    }
                }
                else
                {
                    return GenerateErrorCommandResponse("room does not exists");
                }

                return new ReceiveCommandResponse();
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(ex.Message);
            }
        }

        ReceiveCommandResponse IRoomManager.RemoveUserToRoom(string user, string chatRoom)
        {
            try
            {
                var room = _rooms.Where(room => room.Name.ToUpper().Equals(chatRoom.ToUpper())).FirstOrDefault();
                if (room != null)
                {
                    var selectedUser = _users.Where(item => item.User.NickName.ToUpper().Equals(user.ToUpper())).FirstOrDefault();
                    if (selectedUser != null)
                    {
                        room.Users.RemoveAll(item => item.User.NickName == selectedUser.User.NickName);
                    }
                    else
                    {
                        return GenerateErrorCommandResponse("user does not exists in the room");
                    }
                }
                else
                {
                    return GenerateErrorCommandResponse("room does not exists");
                }

                return new ReceiveCommandResponse();
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(ex.Message);
            }
        }

        private ReceiveCommandResponse BroadCastToRoom(string room, string message)
        {
            try
            {
                var selectedRoom = _rooms.Where(item => item.Name == room).FirstOrDefault();
                if (selectedRoom != null)
                {
                    selectedRoom.Users.ForEach(user => user.SendMessage(message));
                }
                return new ReceiveCommandResponse();
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(ex.Message);
            }
        }
        private ReceiveCommandResponse GenerateErrorCommandResponse(string message)
        {
            return new ReceiveCommandResponse()
            {
                Type = TypeReceiveCommandResponse.Error,
                MessageError = message
            };
        }
    }
}
