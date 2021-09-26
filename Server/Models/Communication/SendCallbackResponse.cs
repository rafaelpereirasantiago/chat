using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models.Communication
{
    public enum TypeSendCallbackResponse
    {
        Successful = 1,
        Error = 2
    }
    public class SendCallbackResponse
    {
        public TypeSendCallbackResponse Type { get; set; }
        public string MessageError { get; set; } = "";
    }
}
