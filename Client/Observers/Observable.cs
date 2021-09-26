using Client.Interfaces.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Observers
{
    public class Observable: IObservable<string>
    {
        private List<IObserver<string>> observers = new List<IObserver<string>>();

        protected void notifyObservers(string dataReceived)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(dataReceived);
            }
        }

        IDisposable IObservable<string>.Subscribe(IObserver<string> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<string>(observers, observer);
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
