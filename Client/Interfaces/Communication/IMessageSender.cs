using Client.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Interfaces.Communication
{
    public interface IMessageSender
    {
        SendCommandResponse SendPublicMessageForUser(string user, string message);
        SendCommandResponse SendPublicMessage(string message);
    }
}
