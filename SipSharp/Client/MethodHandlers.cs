using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Client
{
    /// <summary>
    /// Used to keep track of which handlers want to get wich method.
    /// </summary>
    public class MethodHandlers
    {
        Dictionary<string, List<EventHandler<RequestEventArgs>>> _items = new Dictionary<string, List<EventHandler<RequestEventArgs>>>();

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

        public bool Invoke(string method, string source, RequestEventArgs args)
        {
            List<EventHandler<RequestEventArgs>> items = GetList(method);
            if (items == null)
                return false;
            

        }

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
    }
}
