using System.Net;
using System.Threading;
using SipSharp;
using SipSharp.Logging;
using SipSharp.Servers.Registrar;

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

            var stack = new SipStack();
            stack.Start(new IPEndPoint(IPAddress.Any, 5060));

            var repos = new RegistrationReporsitory();
            repos.Add("test3", "127.0.0.1");

            var registrar = new Registrar(stack, repos);
            registrar.Domain = new SipUri(null, "pbx.gateon.se");
            registrar.Realm = "pbx.gateon.se";

            Thread.Sleep(500000);
        }
    }
}