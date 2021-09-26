using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Communication.Websocket
{
    public class WebsocketUserConnection : IUserConnection
    {
        private ICommandInterpreter _commandInterpreter;
        private User _user;
        private WebsocketConnectionObject _connectionInfo;
        private bool _active = false;

        User IUserConnection.User { get => _user; }
        ProtocolConnection IUserConnection.Protocol { get => ProtocolConnection.TCP; }

        public WebsocketUserConnection(
            ICommandInterpreter commandInterpreter,
            object connectionObject,
            User user)
        {
            _user = user;
            _connectionInfo = (WebsocketConnectionObject)connectionObject;
            _commandInterpreter = commandInterpreter;
        }

        void IUserConnection.Listen()
        {
            string[] prefixes = { $"http://{_connectionInfo.IP}:{_connectionInfo.Port}/{_user.ClientId}/" };

            HttpListener listener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }

            listener.Start();

            _active = true;
            Thread _tcpListenerThread = new Thread(() =>
            {
                while (_active)
                {
                    try
                    {
                        ReadMessage(listener).Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        private async Task ReadMessage(HttpListener listener)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            if (request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;
                string dataReceived = await GetWebsocketData(webSocket);
                //await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();

                _commandInterpreter.Interpret(dataReceived);
            }
            else
            {
                GenerateBadRequest(context);
            }
        }

        public void SendMessage(string message)
        {
            var uri = new Uri($"ws://{_connectionInfo.UserHostAddress}:{_user.CallbackPort}/{_user.ClientId}/callback/");
            ClientWebSocket client = new ClientWebSocket();
            var source = new CancellationTokenSource();
            source.CancelAfter(5000);
            client.ConnectAsync(uri, source.Token).Wait();

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            client.SendAsync(bytesToSend, WebSocketMessageType.Text,true, source.Token).Wait();
        }

        async Task<string> GetWebsocketData(WebSocket socket)
        {
            var Token = CancellationToken.None;
            string message = "";

            byte[] buffer = new byte[4096];
            while (socket.State == WebSocketState.Open && !Token.IsCancellationRequested)
            {
                WebSocketReceiveResult receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), Token);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", Token);
                }
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    message += Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                }

                if (receiveResult.EndOfMessage)
                    break;
            }

            return message;
        }

        public void Sutdown()
        {
            _active = false;
        }

        void IUserConnection.NotifyUserAlreadyExists()
        {
            NotifyUser(_connectionInfo.Socket, _connectionInfo.Response, "user-already-exists");
        }

        void IUserConnection.NotifyRegisteredUser()
        {
            NotifyUser(_connectionInfo.Socket, _connectionInfo.Response, "ok");
        }

        private void NotifyUser(WebSocket socket, HttpListenerResponse response, string message)
        {
            response.StatusCode = (int)HttpStatusCode.OK;

            var encoded = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            response.Close();
        }


        private void GenerateBadRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
        }
    }
}
