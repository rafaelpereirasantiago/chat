using Client.Models.Communication;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using Server.Observers;
using System;
using System.Net;
using System.Threading;

namespace Server.Communication.Http
{
    class HttpHandleClientConnection : Observable, IHandleClientConnection
    {
        HttpListener _listener = new HttpListener();
        ListnerConnectionParameters _parameters;
        private bool _active;
        ProtocolConnection IHandleClientConnection.Protocol => ProtocolConnection.HTTP;

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
                        ReadMessage();
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
                    Protocol = ProtocolConnection.HTTP,
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

        void ReadMessage()
        {
            HttpListenerContext context = _listener.GetContext();
            HttpListenerRequest request = context.Request;

            string dataReceived = GetRequestData(request);

            //context.Response.StatusCode = (int)HttpStatusCode.OK;
            //context.Response.Close();

            if (IsRegisterUserMessage(dataReceived))
            {
                var userHostAddress = request.UserHostAddress.Split(":")[0];
                var connectionObject = new HttpConnectionObject(context.Response, userHostAddress, _parameters.IP, _parameters.Port);
                NotifyNewUser(connectionObject, dataReceived);
            }
        }
    }
}
