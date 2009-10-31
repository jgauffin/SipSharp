using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp
{
    /// <summary>
    /// Purpose of this class is to manage a list of subscribers of some kind.
    /// </summary>
    class SubscriberList<T>
    {
        private Dictionary<string, LinkedList<T>> _subscribers =
            new Dictionary<string, LinkedList<T>>();

        private LinkedList<T> _allHandlers = new LinkedList<T>();

        public void Register(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                lock (_allHandlers)
                    _allHandlers.AddLast(value);
                return;
            }

            LinkedList<T> subscribers;
            lock (_subscribers)
            {
                if (!_subscribers.TryGetValue(key, out subscribers))
                {
                    subscribers = new LinkedList<T>();
                    _subscribers.Add(key, subscribers);
                }
            }

            lock (subscribers)
                subscribers.AddLast(value);            
        }

        public bool Invoke(string key, Action<T> action)
        {
            LinkedList<T> subscribers;
            lock (_subscribers)
            {
                if (!_subscribers.TryGetValue(key, out subscribers))
                    subscribers = null;
            }

            if (subscribers != null)
            {
                lock (subscribers)
                {
                    foreach (T subscriber in subscribers)
                        action(subscriber);
                }
            }

            lock (_allHandlers)
            {
                foreach (var handler in _allHandlers)
                    action(handler);
            }

            return true;
        }
    }
}
