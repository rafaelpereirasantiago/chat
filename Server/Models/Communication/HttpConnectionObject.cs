using System.Net;

namespace Server.Models.Communication
{
    internal class HttpConnectionObject
    {
        public string UserHostAddress { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public HttpListenerResponse Response { get; set; }

        public HttpConnectionObject(HttpListenerResponse response, string userHostAddress, string ip, int port)
        {
            IP = ip;
            Response = response;
            UserHostAddress = userHostAddress;
            Port = port;
        }
    }
}