using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages;
using SipSharp.Transactions;

namespace SipSharp.Client
{
    class SipClient
    {
        private readonly ISipStack _stack;
        private Contact _contact;

        public SipClient(ISipStack stack)
        {
            _stack = stack;
            
        }

        public void MakeCall(Contact to)
        {


        }
    }
}
