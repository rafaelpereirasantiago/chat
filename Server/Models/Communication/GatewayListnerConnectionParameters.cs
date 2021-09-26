using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.Communication
{
    public class GatewayListnerConnectionParameters
    {
        public string IP { get; set; }
        public int HttpPort { get; set; }
        public int TcpPort { get; set; }
        public int WebsocketPort { get; set; }
    }
}
