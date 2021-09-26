using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Communication
{
    public enum TypeSendCommandResponse
    {
        Successful = 1,
        Error = 2
    }
    public class SendCommandResponse
    {
        public TypeSendCommandResponse Type { get; set; }
        public string Command { get; set; } = "";
        public string MessageError { get; set; } = "";
        public string ResponseMessage { get; set; } = "";
    }
}
