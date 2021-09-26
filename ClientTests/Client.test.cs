using Client;
using Client.Communication;
using Client.Interfaces;
using Client.Interfaces.Communication;
using Client.Models;
using Client.Models.Communication;
using ClientTests.Mocks;
using NUnit.Framework;

namespace ClientTests
{
    public class Tests
    {
        private IChat _chat;
        private ICommandInterpreter _commandInterpreter;
        private User _user;

        [OneTimeSetUp]
        public void Setup()
        {
            IServerConnection serverConnection = new ServerConnectionMock();

            ServerConnectionParameters serverConnectionParameters = new ServerConnectionParameters()
            {
                IP = "127.0.0.1",
                Port = 8002
            };

            _chat = new Chat(
                serverConnection,
                serverConnectionParameters
            );

            //_chat.Initialize();

            _commandInterpreter = new CommandInterpreter(_chat);

            _user = new User()
            {
                IP = "127.0.0.1",
                Port = 0,
                NickName = "NickNameTest"
            };
        }

        [Test, Order(1)]
        public void Initialize()
        {
            _chat.Initialize();

            Assert.AreEqual(_chat.Connected, true);
        }

        [Test, Order(2)]
        public void RegisterUser()
        {
            SendCommandResponse response = _chat.RegisterUser(_user);

            Assert.AreEqual($"/register-user {_user.NickName} {_user.IP} {_user.Port} {_user.ClientID} {_user.CallbackPort}", response.Command);
            Assert.AreEqual(response.ResponseMessage, "ok");
        }

        [Test, Order(3)]
        public void RegisterWrongUser()
        {
            var wrongUser = new User()
            {
                IP = "127.0.0.1",
                Port = 0,
                NickName = "wrongUser"
            };
            SendCommandResponse response = _chat.RegisterUser(wrongUser);

            Assert.AreEqual($"/register-user {wrongUser.NickName} {wrongUser.IP} {wrongUser.Port} {wrongUser.ClientID} {wrongUser.CallbackPort}", response.Command);
            Assert.AreEqual(response.ResponseMessage, "user-already-exists");
        }

        [Test, Order(4)]
        public void SendPublicMessage()
        {
            string message = "test message";
            SendCommandResponse response = _commandInterpreter.Interpret($"/m {message}");

            Assert.AreEqual($"/m {_user.NickName} {_chat.SelectedRoom} {message}", response.Command);
        }

        [Test, Order(5)]
        public void SendPublicMessageForUser()
        {
            string message = "test message";
            string user = "user";
            SendCommandResponse response = _commandInterpreter.Interpret($"/p {user} {message}");

            Assert.AreEqual($"/p {_user.NickName} {_chat.SelectedRoom} {user} {message}", response.Command);
        }

        [Test, Order(6)]
        public void SelectRoom()
        {
            string room = "#room";
            SendCommandResponse response = _commandInterpreter.Interpret($"/select-room {room}");

            Assert.AreEqual($"/select-room {_user.NickName} {room}", response.Command);
            Assert.AreEqual(_chat.SelectedRoom, room);
        }

        [Test, Order(7)]
        public void CreateRoom()
        {
            string room = "#room";
            SendCommandResponse response = _commandInterpreter.Interpret($"/create-room {room}");

            Assert.AreEqual($"/create-room {_user.NickName} {room}", response.Command);
            Assert.AreEqual(_chat.SelectedRoom, room);
        }

        [Test, Order(8)]
        public void InvalidCommand()
        {
            SendCommandResponse response = _commandInterpreter.Interpret($"/invalid-command");

            Assert.AreEqual(TypeSendCommandResponse.Error, response.Type);
            Assert.AreEqual("Invalid Command", response.MessageError);
        }

        [Test, Order(9)]
        public void Quit()
        {
            SendCommandResponse response = _commandInterpreter.Interpret($"/exit");

            Assert.AreEqual($"/exit {_user.NickName}", response.Command);
            Assert.AreEqual(_chat.Connected, false);
        }
    }
}