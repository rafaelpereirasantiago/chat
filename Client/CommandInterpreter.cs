using Client.Interfaces;
using Client.Interfaces.Communication;
using Client.Models;
using Client.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class CommandInterpreter: ICommandInterpreter
    {
        IChat _chat;
        Dictionary<Func<string, bool>, Func<string, SendCommandResponse>> commandList = new Dictionary<Func<string, bool>, Func<string, SendCommandResponse>>();

        public CommandInterpreter(IChat chat)
        {
            _chat = chat;

            DefineCommandDefinitions();
        }

        private void DefineCommandDefinitions()
        {
            commandList.Add(IsSendPublicMessage, SendPublicMessage);
            commandList.Add(IsSendPublicMessageForUser, SendPublicMessageForUser);
            commandList.Add(IsQuit, SendQuit);
            commandList.Add(IsSelectRoom, SelectRoom);
            commandList.Add(IsCreateRoom, CreateRoom);
        }

        SendCommandResponse ICommandInterpreter.Interpret(string command)
        {
            foreach (var item in commandList)
            {
                if (item.Key(command))
                {
                    return item.Value(command);
                }
            }

            return new SendCommandResponse() {
                Type = TypeSendCommandResponse.Error,
                Command = command,
                MessageError = "Invalid Command"
            };
        }

        private bool IsSendPublicMessage(string command)
        {
            return command.StartsWith("/m ");
        }

        private SendCommandResponse SendPublicMessage(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 2)
            {
                string message = command.Replace($"{parameters[0]} ", "");
                return _chat.SendPublicMessage(message);
            }

            return null;
        }

        private bool IsSendPublicMessageForUser(string command)
        {
            return command.StartsWith("/p ");
        }

        private SendCommandResponse SendPublicMessageForUser(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 3)
            {
                string user = parameters[1];
                string message = command.Replace($"{parameters[0]} {user} ", "");
                return _chat.SendPublicMessageForUser(user, message);
            }

            return null;
        }

        private bool IsQuit(string command)
        {
            return command.StartsWith("/exit");
        }

        private SendCommandResponse SendQuit(string command)
        {
            return _chat.Quit();
        }

        private bool IsSelectRoom(string command)
        {
            return command.StartsWith("/select-room ");
        }

        private SendCommandResponse SelectRoom(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 2)
            {
                string room = parameters[1];
                return _chat.SelectRoom(room);
            }

            return null;
        }

        private bool IsCreateRoom(string command)
        {
            return command.StartsWith("/create-room ");
        }

        private SendCommandResponse CreateRoom(string command)
        {
            string[] parameters = command.Split(" ");

            if (parameters.Length >= 2)
            {
                string room = parameters[1];
                return _chat.CreateRoom(new Room()
                {
                    Name = room
                });
            }

            return null;
        }
    }
}
