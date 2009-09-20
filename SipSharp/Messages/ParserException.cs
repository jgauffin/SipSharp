using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Parser
{
    class ParserException : Exception
    {
        public ParserException(string errMsg ) :base(errMsg)
        {
            
        }

        public ParserException(string errMsg, Exception inner)
            : base(errMsg, inner)
        {

        }
    }
}
