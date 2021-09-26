using Server.Models.Communication;
using System;
using System.Collections.Generic;

namespace Server.Observers
{
    public class Observable: IObservable<HandleClientMessage>
    {
        private List<IObserver<HandleClientMessage>> observers = new List<IObserver<HandleClientMessage>>();

        protected void notifyObservers(HandleClientMessage dataReceived)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(dataReceived);
            }
        }

        IDisposable IObservable<HandleClientMessage>.Subscribe(IObserver<HandleClientMessage> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<HandleClientMessage>(observers, observer);
        }
    }


    class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
