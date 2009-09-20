using System;

namespace SipSharp.Messages
{
    /// <summary>
    /// Parsing failed.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="errMsg">The err MSG.</param>
        public ParseException(string errMsg) : base(errMsg)
        {
            
        }

        public ParseException(string errMsg, Exception inner):base(errMsg, inner)
        {
            
        }
    }
}
