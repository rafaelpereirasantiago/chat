using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.Communication
{
    public enum StatusConnection
    {
        Successful = 1,
        Error = 2,
    }

    public class ConnectionResponse
    {
        public StatusConnection Status { get; set; }
        public String MessageError { get; set; } = "";
    }
}
