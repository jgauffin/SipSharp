using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SipSharp;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Servers;
using SipSharp.Servers.Registrar;

namespace SwitchSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFilter filter = new LogFilter();
            filter.AddNamespace("SipSharp.Transports.*", LogLevel.Debug);
            filter.AddStandardRules();

            LogFactory.Assign(new ConsoleLogFactory(filter));

            SipStack stack = new SipStack();
            stack.Start(new IPEndPoint(IPAddress.Any, 5060));

            RegistrationReporsitory repos = new RegistrationReporsitory();
            repos.Add("test3", "127.0.0.1");

            Registrar registrar = new Registrar(stack, repos);
            registrar.Domain = new SipUri(null, "pbx.gateon.se");
            registrar.Realm = "pbx.gateon.se";

            Thread.Sleep(500000);
        }
    }
}
