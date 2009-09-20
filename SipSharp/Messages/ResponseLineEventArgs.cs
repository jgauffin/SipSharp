using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Transports.Parser
{
    /// <summary>
    /// First line in a response have been received
    /// </summary>
    public class ResponseLineEventArgs : EventArgs
    {

        /// <summary>
        /// Gets or sets message status code
        /// </summary>
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets motivation to why the status code was used.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets sip protocol version used.
        /// </summary>
        public string Version { get; set; }
    }
}
