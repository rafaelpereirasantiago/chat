using Client.Interfaces.Communication;
using Client.Observers;
using Client.Models.Communication;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.Http;

namespace Client.Communication.Http
{
    public class HttpServerConnection: Observable, IServerConnection
    {
        private HttpListener _listener = new HttpListener();
        private bool _listen = false;
        private string _serverUri = "";
        private bool _connected = false;

        bool IServerConnection.Connected => _connected;

        bool IServerConnection.isListenServer => _listen;

        ConnectionResponse IServerConnection.Connect(ServerConnectionParameters parameters)
        {
            _serverUri = $"http://{parameters.IP}:{parameters.Port}/{parameters.ClientID}/";

            string[] prefixes = { $"http://{parameters.LocalIP}:{parameters.CallbackPort}/{parameters.ClientID}/callback/" };
            foreach (string prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _connected = true;
            return new ConnectionResponse();
        }

        HttpResponseMessage SendMessage(string message)
        {
            HttpClient client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _serverUri);
            requestMessage.Content = new StringContent(message, Encoding.UTF8, "text/html");
            return client.Send(requestMessage);
        }

        SendCommandResponse IServerConnection.SendCommand(string command)
        {
            try
            {
                var response = SendMessage(command);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return GenerateSuccessfulCommandResponse(command);
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

        SendCommandResponse IServerConnection.SendCommandWithCallback(string command)
        {
            try
            {
                var response = SendMessage(command);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new SendCommandResponse()
                    {
                        Type = TypeSendCommandResponse.Successful,
                        Command = command,
                        ResponseMessage = ReadResponseContent(response.Content)
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

        private void ListenServer()
        {
            HttpListenerContext context = _listener.GetContext();
            HttpListenerRequest request = context.Request;

            string dataReceived = GetRequestData(request);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();

            notifyObservers(dataReceived);
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
                        ListenServer();
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
        string GetRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return "";
            }
            using (System.IO.Stream body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private string ReadResponseContent(HttpContent content)
        {
            return content.ReadAsStringAsync().Result;
        }
    }
}
