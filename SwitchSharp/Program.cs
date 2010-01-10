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

            var repos = new RegistrationRepository();
            repos.Add(new SipUri("0706930821@mydomain.com"), "u1000067");

            SwitchSharp switchSharp = new SwitchSharp();
            switchSharp.RegistrationDatabase = repos;
            switchSharp.AddListener(new UdpTransport(new IPEndPoint(IPAddress.Any, 5060), switchSharp.MessageFactory));
            switchSharp.Start("mydomain.com");
            

            Thread.Sleep(500000);
        }
    }
}