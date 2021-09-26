using System;

namespace Client.Observers
{
    class ServerConnectionObserver : IObserver<string>
    {
        void IObserver<string>.OnCompleted()
        {
            
        }

        void IObserver<string>.OnError(Exception error)
        {
            
        }

        void IObserver<string>.OnNext(string value)
        {
            Console.WriteLine(value);
        }
    }
}
