using System.Net;
using System.Net.WebSockets;

namespace Server.Models.Communication
{
    internal class WebsocketConnectionObject
    {
        public string UserHostAddress { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public HttpListenerResponse Response { get; set; }
        public WebSocket Socket { get; set; }

        public WebsocketConnectionObject(
            HttpListenerResponse response,
            WebSocket socket,
            string userHostAddress, 
            string ip, 
            int port
        )
        {
            Socket = socket;
            IP = ip;
            Response = response;
            UserHostAddress = userHostAddress;
            Port = port;
        }
    }
}