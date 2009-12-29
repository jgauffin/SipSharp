using System.Collections.Generic;
using System.Collections.Specialized;

namespace SipSharp
{
    public class HeaderValue
    {
        private IList<string> _options;
        private NameValueCollection _parameters;

        public HeaderValue(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        internal void Append(string value)
        {
            Value += value;
        }
    }
}