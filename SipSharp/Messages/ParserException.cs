using System;

namespace SipSharp.Parser
{
    internal class ParserException : Exception
    {
        public ParserException(string errMsg) : base(errMsg)
        {
        }

        public ParserException(string errMsg, Exception inner)
            : base(errMsg, inner)
        {
        }
    }
}