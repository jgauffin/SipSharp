using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Calls
{
    public class CallManager
    {
        public CallManager(ISipStack stack)
        {
            stack.RequestReceived += OnRequest;
            stack.ResponseReceived += OnResponse;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void MakeCall(Contact from, Contact to)
        {
            
        }
    }
}
