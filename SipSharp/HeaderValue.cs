using System.Collections.Generic;
using System.Collections.Specialized;

namespace SipSharp
{
    public class HeaderValue
    {
        private string _value;
        private NameValueCollection _parameters;
        private IList<string> _options;

        public HeaderValue(string value)
        {
            Value = value;
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        internal void Append(string value)
        {
            Value += value;
        }
    }
}
