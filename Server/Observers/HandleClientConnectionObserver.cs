using Server.Interfaces;
using Server.Models.Communication;
using System;

namespace Server.Observers
{
    class HandleClientConnectionObserver : IObserver<HandleClientMessage>
    {
        private IServer _server;
        public HandleClientConnectionObserver(IServer server)
        {
            _server = server;
        }

        void IObserver<HandleClientMessage>.OnCompleted()
        {
            
        }

        void IObserver<HandleClientMessage>.OnError(Exception error)
        {
            
        }

        void IObserver<HandleClientMessage>.OnNext(HandleClientMessage value)
        {
            _server.RegisterUser(
                value.Protocol,
                value.ConnectionObject,
                value.User
            );
        }
    }
}
