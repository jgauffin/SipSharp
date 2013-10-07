using System;

namespace SipSharp
{
    /// <summary>
    /// Request syntax was incorrect.
    /// </summary>
    public class BadRequestException : SipException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="msg">Why exception was thrown.</param>
        public BadRequestException(string msg) : base(StatusCode.BadRequest, msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class.
        /// </summary>
        /// <param name="msg">Error message.</param>
        /// <param name="inner">Inner exception.</param>
        public BadRequestException(string msg, Exception inner) : base(StatusCode.BadRequest, msg, inner)
        {
        }
    }
}