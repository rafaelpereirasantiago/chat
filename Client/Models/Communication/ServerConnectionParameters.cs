using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Communication
{
    public class ServerConnectionParameters
    {
        public string LocalIP { get; set; } = "";
        public string ClientID { get; set; } = "";
        public string IP { get; set; }
        public int Port { get; set; }

        public int CallbackPort { get; set; }
    }
}
