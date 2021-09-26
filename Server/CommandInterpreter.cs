using Server.Interfaces;
using Server.Models.Communication;
using System;
using System.Collections.Generic;

namespace Server
{
    public class CommandInterpreter: ICommandInterpreter
    {
        IServer _server;
        Dictionary<Func<string, bool>, Func<string, ReceiveCommandResponse>> commandList = new Dictionary<Func<string, bool>, Func<string, ReceiveCommandResponse>>();

        public CommandInterpreter()
        {
            DefineCommandDefinitions();
        }

        IServer ICommandInterpreter.Server { get => _server; set { _server = value; } }

        private void DefineCommandDefinitions()
        {
            commandList.Add(IsSendPublicMessage, SendPublicMessage);
            commandList.Add(IsSendPublicMessageForUser, SendPublicMessageForUser);
            commandList.Add(IsQuit, SendQuit);
            commandList.Add(IsSelectRoom, SelectRoom);
            commandList.Add(IsCreateRoom, CreateRoom);
        }

        ReceiveCommandResponse ICommandInterpreter.Interpret(string command)
        {
            foreach (var item in commandList)
            {
                if (item.Key(command))
                {
                    return item.Value(command);
                }
            }

            return new ReceiveCommandResponse() {
                Type = TypeReceiveCommandResponse.Error,
                MessageError = "Invalid Command"
            };
        }

        private bool IsSendPublicMessage(string command)
        {
            return command.StartsWith("/m ");
        }

        private ReceiveCommandResponse SendPublicMessage(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 3)
            {
                string senderUser = parameters[1];
                string room = parameters[2];
                string message = command.Replace($"{parameters[0]} {senderUser} {room} ", "");
                return _server.ReceivePublicMessage(senderUser, room, message);
            }

            return null;
        }

        private bool IsSendPublicMessageForUser(string command)
        {
            return command.StartsWith("/p ");
        }

        private ReceiveCommandResponse SendPublicMessageForUser(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 4)
            {
                string senderUser = parameters[1];
                string room = parameters[2];
                string user = parameters[3];

                string message = command.Replace($"{parameters[0]} {senderUser} {room} {user} ", "");
                return _server.ReceivePublicMessageForUser(senderUser, room, user, message);
            }

            return null;
        }

        private bool IsQuit(string command)
        {
            return command.StartsWith("/exit");
        }

        private ReceiveCommandResponse SendQuit(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 2)
            {
                string user = parameters[1];
                return _server.Quit(user);
            }
            return null;
        }

        private bool IsSelectRoom(string command)
        {
            return command.StartsWith("/select-room ");
        }

        private ReceiveCommandResponse SelectRoom(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 3)
            {
                string user = parameters[1];
                string room = parameters[2];
                return _server.AddUserToRoom(user, room);
            }

            return null;
        }

        private bool IsCreateRoom(string command)
        {
            return command.StartsWith("/create-room ");
        }

        private ReceiveCommandResponse CreateRoom(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 2)
            {
                string room = parameters[1];
                return _server.CreateRoom(new Room()
                {
                    Name = room
                });
            }

            return null;
        }
    }
}
