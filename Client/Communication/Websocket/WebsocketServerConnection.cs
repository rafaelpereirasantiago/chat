using Client.Interfaces.Communication;
using Client.Observers;
using Client.Models.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Client.Communication.Websocket
{
    public class WebsocketServerConnection : Observable, IServerConnection
    {
        private HttpListener _listener = new HttpListener();
        private bool _listen = false;
        private string _serverUri = "";
        private bool _connected = false;

        bool IServerConnection.Connected => _connected;
        ConnectionResponse IServerConnection.Connect(ServerConnectionParameters parameters)
        {
            _serverUri = $"ws://{parameters.IP}:{parameters.Port}/{parameters.ClientID}/";

            string[] prefixes = { $"http://{parameters.LocalIP}:{parameters.CallbackPort}/{parameters.ClientID}/callback/" };
            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _connected = true;
            return new ConnectionResponse();
        }

        async Task<ClientWebSocket> SendMessage(string message)
        {
            var uri = new Uri(_serverUri);
            ClientWebSocket client = new ClientWebSocket();
            var source = new CancellationTokenSource();
            source.CancelAfter(5000);
            await client.ConnectAsync(uri, source.Token);

            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, source.Token);
            return client;
        }

        SendCommandResponse IServerConnection.SendCommand(string command)
        {
            try
            {
                SendMessage(command).Wait();
                return GenerateSuccessfulCommandResponse(command);
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(command, ex.Message);
            }
        }

        SendCommandResponse IServerConnection.SendCommandWithCallback(string command)
        {
            try
            {
                var webSocket = SendMessage(command).Result;
                var response = GetWebSocketData(webSocket).Result;
                if (response != "")
                {
                    return new SendCommandResponse()
                    {
                        Type = TypeSendCommandResponse.Successful,
                        Command = command,
                        ResponseMessage = response
                    };
                }
                else
                {
                    return GenerateErrorCommandResponse(command, "Invalid response");
                }
            }
            catch (Exception ex)
            {
                return GenerateErrorCommandResponse(command, ex.Message);
            }
        }

        private async Task ListenServer()
        {
            var context = _listener.GetContext();
            var request = context.Request;

            if (request.IsWebSocketRequest)
            {
                var webSocketContext = await context .AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;
                string dataReceived = await GetWebSocketData (webSocket);
                //await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();

                notifyObservers(dataReceived);
            }
            else
            {
                GenerateBadRequest(context);
            }
        }

        void IServerConnection.Listen()
        {
            _listener.Start();

            _listen = true;
            Thread _tcpListenerThread = new Thread(() =>
            {
                while (_listen)
                {
                    try
                    {
                        ListenServer().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        public void StopListen()
        {
            _listen = false;
        }

        private SendCommandResponse GenerateErrorCommandResponse(string command, string message)
        {
            return new SendCommandResponse()
            {
                Type = TypeSendCommandResponse.Error,
                Command = command,
                MessageError = message
            };
        }

        private SendCommandResponse GenerateSuccessfulCommandResponse(string command)
        {
            return new SendCommandResponse()
            {
                Type = TypeSendCommandResponse.Successful,
                Command = command
            };
        }

        void IServerConnection.Disconnect()
        {
            StopListen();
            _connected = false;
        }
        async Task<string> GetWebSocketData(WebSocket socket)
        {
            var Token = CancellationToken.None;
            string message = "";

            byte[] buffer = new byte[4096];
            while (socket.State == WebSocketState.Open && !Token.IsCancellationRequested)
            {
                WebSocketReceiveResult receiveResult = socket.ReceiveAsync(new ArraySegment<byte>(buffer), Token).Result;
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

        private void GenerateBadRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
        }
    }
}
