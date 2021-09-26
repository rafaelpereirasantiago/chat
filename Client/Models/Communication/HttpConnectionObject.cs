using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Communication
{
    public class HttpConnectionObject
    {

        public HttpConnectionObject(string host, int port)
        {
            Host = host;
            Port = port;
        }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
