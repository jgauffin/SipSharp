using System.Net;
using System.Threading;
using SipSharp;
using SipSharp.Logging;
using SipSharp.Servers.Registrar;
using SipSharp.Transports;

namespace SwitchSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var filter = new LogFilter();
            filter.AddNamespace("SipSharp.Transports.*", LogLevel.Debug);
            filter.AddStandardRules();

            LogFactory.Assign(new ConsoleLogFactory(filter));

            var repos = new RegistrationReporsitory();
            repos.Add("test3", "127.0.0.1");

            SwitchSharp switchSharp = new SwitchSharp();

            switchSharp.AddListener(new UdpTransport(switchSharp.MessageFactory));
            switchSharp.Start("mydomain.com");
            

            Thread.Sleep(500000);
        }
    }
}