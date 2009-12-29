using System.Net;

namespace SipSharp
{
    public class Hop
    {
        public Hop(string transport, IPEndPoint ep)
        {
            Transport = transport;
            EndPoint = ep;
        }

        /// <summary>
        /// Gets target IP address.
        /// </summary>
        public IPAddress Address
        {
            get { return EndPoint.Address; }
        }

        /// <summary>
        /// Gets target IP end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets target port.
        /// </summary>
        public int Port
        {
            get { return EndPoint.Port; }
        }

        /// <summary>
        /// Gets target SIP transport.
        /// </summary>
        public string Transport { get; private set; }
    }
}