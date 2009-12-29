using System;
using System.Collections.Generic;
using System.Threading;

namespace SipSharp.Servers.Registrar
{
    internal class NonceManager
    {
        private readonly Dictionary<string, DateTime> _nonces = new Dictionary<string, DateTime>();
        private Timer _timer;

        public NonceManager()
        {
            _timer = new Timer(RemoveOld, null, 15000, 15000);
        }

        /// <summary>
        /// Gets the current nonce.
        /// </summary>
        /// <returns></returns>
        public virtual string Create()
        {
            string nonce = Guid.NewGuid().ToString().Replace("-", string.Empty);
            lock (_nonces)
                _nonces.Add(nonce, DateTime.Now.AddSeconds(30));

            return nonce;
        }


        /// <summary>
        /// determines if the nonce is valid or has expired.
        /// </summary>
        /// <param name="value">nonce value (check wikipedia for info)</param>
        /// <returns>true if the nonce has not expired.</returns>
        public virtual bool IsValid(string value)
        {
            lock (_nonces)
            {
                DateTime expires;
                if (!_nonces.TryGetValue(value, out expires))
                    return false;

                if (_nonces[value] < DateTime.Now)
                {
                    _nonces.Remove(value);
                    return false;
                }
            }

            return true;
        }

        private void RemoveOld(object state)
        {
            lock (_nonces)
            {
                foreach (var pair in _nonces)
                {
                    if (pair.Value >= DateTime.Now)
                        continue;

                    _nonces.Remove(pair.Key);
                    return;
                }
            }
        }
    }
}