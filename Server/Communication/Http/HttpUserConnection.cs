using Server.Interfaces;
using Server.Interfaces.Communication;
using Server.Models.Communication;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Server.Communication.Http
{
    public class HttpUserConnection : IUserConnection
    {
        private ICommandInterpreter _commandInterpreter;
        private User _user;
        private HttpConnectionObject _connectionInfo;
        private bool _active = false;

        User IUserConnection.User { get => _user; }
        ProtocolConnection IUserConnection.Protocol { get => ProtocolConnection.TCP; }

        public HttpUserConnection(
            ICommandInterpreter commandInterpreter,
            object connectionObject,
            User user)
        {
            _user = user;
            _connectionInfo = (HttpConnectionObject)connectionObject;
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
                        ReadMessage(listener);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
            _tcpListenerThread.Start();
        }

        private void ReadMessage(HttpListener listener)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;

            string dataReceived = GetRequestData(request);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();

            _commandInterpreter.Interpret(dataReceived);
        }

        public void SendMessage(string message)
        {
            string uri = $"http://{_connectionInfo.UserHostAddress}:{_user.CallbackPort}/{_user.ClientId}/callback/";
            HttpClient client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Content = new StringContent(message, Encoding.UTF8, "text/html");
            client.Send(requestMessage);
        }

        string GetRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return "";
            }
            using (Stream body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public void Sutdown()
        {
            _active = false;
        }

        void IUserConnection.NotifyUserAlreadyExists()
        {
            NotifyUser(_connectionInfo.Response, "user-already-exists");
        }

        void IUserConnection.NotifyRegisteredUser()
        {
            NotifyUser(_connectionInfo.Response, "ok");
        }

        private void NotifyUser(HttpListenerResponse response, string message)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            response.ContentLength64 = buffer.Length;
            Stream st = response.OutputStream;
            st.Write(buffer, 0, buffer.Length);

            response.Close();
        }
    }
}
