using System;
using System.Collections.Generic;

namespace SipSharp.Client
{
    /// <summary>
    /// Used to keep track of which handlers want to get which method.
    /// </summary>
    public class MethodHandlers
    {
        private readonly Dictionary<string, List<EventHandler<RequestEventArgs>>> _items =
            new Dictionary<string, List<EventHandler<RequestEventArgs>>>();

        private List<EventHandler<RequestEventArgs>> GetList(string method)
        {
            List<EventHandler<RequestEventArgs>> items;
            lock (_items)
                return _items.TryGetValue(method, out items) ? items : null;
        }

        private List<EventHandler<RequestEventArgs>> GetListOrCreateIt(string method)
        {
            List<EventHandler<RequestEventArgs>> items;
            lock (_items)
            {
                if (!_items.TryGetValue(method, out items))
                {
                    items = new List<EventHandler<RequestEventArgs>>();
                    _items.Add(method, items);
                }
            }

            return items;
        }

        public bool Invoke(string method, string source, RequestEventArgs args)
        {
            List<EventHandler<RequestEventArgs>> items = GetList(method);
            if (items == null)
                return false;

            lock (items)
                foreach (var handler in items)
                    handler(source, args);
            return items.Count > 0;
        }

        public void Register(string method, EventHandler<RequestEventArgs> handler)
        {
            List<EventHandler<RequestEventArgs>> items = GetListOrCreateIt(method);
            lock (items)
                items.Add(handler);
        }

        public void Remove(string method, EventHandler<RequestEventArgs> handler)
        {
            List<EventHandler<RequestEventArgs>> items = GetList(method);
            if (items == null)
                return;

            lock (items)
                items.Remove(handler);
        }
    }
}