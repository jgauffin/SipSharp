using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Logging;
using SipSharp.Messages;

namespace SipSharp.Client
{
    class UserAgentServer
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
