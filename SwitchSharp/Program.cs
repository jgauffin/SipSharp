using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SipSharp;
using SipSharp.Logging;
using SipSharp.Servers;

namespace SwitchSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.Assign(ConsoleLogFactory.Instance);
            SipStack stack = new SipStack();
            stack.Start(new IPEndPoint(IPAddress.Any, 5060));
            Registrar registrar = new Registrar(stack);
            Thread.Sleep(500000);
        }
    }
}
