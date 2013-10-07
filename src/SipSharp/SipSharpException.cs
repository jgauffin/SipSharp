using System;

namespace SipSharp
{
    public class SipSharpException : Exception
    {
        public SipSharpException(string errMsg) : base(errMsg)
        {
        }
    }
}