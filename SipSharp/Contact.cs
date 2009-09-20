using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SipSharp
{
    public class Contact
    {
        private IKeyValueCollection _parameters;

        public Contact(IKeyValueCollection parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Gets or sets contact's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets where contact can be reached
        /// </summary>
        public SipUri Uri { get; set; }

        public IKeyValueCollection Parameters
        {
            get {
                return _parameters;
            }
        }

        public string GetParameter(string name)
        {
            return _parameters[name];
        }

        public bool HasParameter(string name)
        {
            return _parameters.Contains(name);
        }


    }
}
