using System;
using SipSharp.Logging;

namespace SipSharp.Client
{
    internal class UserAgentServer
    {
        private ILogger _logger = LogFactory.CreateLogger(typeof (UserAgentServer));

        /// <summary>
        /// Received a request from the other end.
        /// </summary>
        /// <param name="request"></param>
        public void OnRequest(object sender, RequestEventArgs e)
        {
        }


        public void Subscribe(string method, EventHandler<ResponseEventArgs> handler)
        {
        }
    }
}