using Client.Models.Communication;
using Server.Communication;
using Server.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Interfaces.Communication
{
    public interface IHandleClientConnection : IObservable<HandleClientMessage>
    {
        ProtocolConnection Protocol { get; }
        bool Active { get; }
        void Start(ListnerConnectionParameters parameters);
        void Close();
    }
}
