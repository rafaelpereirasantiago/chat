using Client.Models.Communication;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using Server.Observers;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Communication.Websocket
{
    class WebsocketHandleClientConnection : Observable, IHandleClientConnection
    {
        HttpListener _listener = new HttpListener();
        ListnerConnectionParameters _parameters;
        private bool _active;
        ProtocolConnection IHandleClientConnection.Protocol => ProtocolConnection.WebSocket;

        bool IHandleClientConnection.Active => _active;

        void IHandleClientConnection.Close()
        {
            throw new NotImplementedException();
        }

        void IHandleClientConnection.Start(ListnerConnectionParameters parameters)
        {
            _parameters = parameters;
            string[] prefixes = { $"http://{parameters.IP}:{parameters.Port}/" };

            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }

            _listener.Start();

            _active = true;
            Thread _tcpListenerThread = new Thread(() =>
            {
                while (_active)
                {
                    try
                    {
                        ReadMessage().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        private bool IsRegisterUserMessage(string message)
        {
            return message.StartsWith("/register-user ");
        }

        private void NotifyNewUser(object connectionObject, string message)
        {
            string[] parameters = message.Split(" ");

            if (parameters.Length >= 6)
            {
                string nickName = parameters[1];
                string clientId = parameters[4];
                string callbackPort = parameters[5];

                var handleClienteMessage = new HandleClientMessage()
                {
                    Protocol = ProtocolConnection.WebSocket,
                    ConnectionObject = connectionObject,
                    User = new User()
                    {
                        ClientId = clientId,
                        CallbackPort = int.Parse(callbackPort),
                        NickName = nickName
                    }
                };

                notifyObservers(handleClienteMessage);
            }
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

        async Task ReadMessage()
        {
            var context = _listener.GetContext();
            HttpListenerRequest request = context.Request;

            if (request.IsWebSocketRequest)
            {
                //var wsContext = context.AcceptWebSocketAsync(null).Result;

                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;
                string dataReceived = await GetWebsocketData(webSocket);
                if (IsRegisterUserMessage(dataReceived))
                {
                    var userHostAddress = request.UserHostAddress.Split(":")[0];
                    var connectionObject = new WebsocketConnectionObject(context.Response, webSocket, userHostAddress, _parameters.IP, _parameters.Port);
                    NotifyNewUser(connectionObject, dataReceived);
                }
                else
                {
                    GenerateBadRequest(context);
                }

                //await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
            }
            else
            {
                GenerateBadRequest(context);
            }
        }

        private void GenerateBadRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
        }

    }
}
