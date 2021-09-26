using Server.Communication.Http;
using Server.Communication.Tcp;
using Server.Communication.Websocket;
using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Communication
{
    using CreateConnectionFunctionType = Func<ICommandInterpreter, object, User, IUserConnection>;

    class UserConnectionFactory
    {
        public static IUserConnection Create(
            ProtocolConnection type, 
            ICommandInterpreter commandInterpreter,
            object connectionObject,
            User user)
        {
            var commandList = UserConnectionFactory.Definitions();

            foreach (var item in commandList)
            {
                if (item.Key(type))
                {
                    return item.Value(commandInterpreter, connectionObject, user);
                }
            }

            throw new Exception("Invalid type connection");
        }

        private static Dictionary<Func<ProtocolConnection, bool>, CreateConnectionFunctionType> Definitions()
        {
            var commandList = new Dictionary<Func<ProtocolConnection, bool>, CreateConnectionFunctionType>();

            commandList.Add(
                (ProtocolConnection type) => type == ProtocolConnection.TCP, 
                (ICommandInterpreter commandInterpreter, object connectionObject, User user) => new TCPUserConnection(commandInterpreter, connectionObject, user)
            );
            commandList.Add(
                (ProtocolConnection type) => type == ProtocolConnection.HTTP,
                (ICommandInterpreter commandInterpreter, object connectionObject, User user) => new HttpUserConnection(commandInterpreter, connectionObject, user)
            );
            commandList.Add(
                (ProtocolConnection type) => type == ProtocolConnection.WebSocket,
                (ICommandInterpreter commandInterpreter, object connectionObject, User user) => new WebsocketUserConnection(commandInterpreter, connectionObject, user)
            );

            return commandList;
        }
    }
}
