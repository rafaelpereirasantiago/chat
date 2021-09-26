using Server.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.Communication
{
    public class HandleClientMessage
    {
        public ProtocolConnection Protocol { get; set; }
        public object ConnectionObject { get; set; }
        public User User { get; set; }
    }
}
