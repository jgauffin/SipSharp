using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Tools;

namespace SipSharp.Transports
{
    /// <summary>
    /// Responsible of transporting messages to/from end points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The transport manager inspects messages to be able to determine
    /// where they should be send. It will then serialize the message
    /// and send it using the appropiate protocol. It will automatically
    /// create a new socket if no socket exists to the destination.
    /// </para>
    /// <para>
    /// The manager will switch protocol from UDP to TCP if the max UDP packet
    /// size is exceeded.
    /// </para>
    /// <para>
    /// Incoming messages are handled by the <see cref="MessageFactory"/>. Hook
    /// its events to be able to handle them.
    /// </para>
    /// </remarks>
    internal class TransportLayer : ITransportLayer
    {
        public const int UdpMaxSize = 65507;

        private readonly ObjectPool<byte[]> _buffers = new ObjectPool<byte[]>(() => new byte[131072]);
                                            // double size to support large messages.

        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (TransportLayer));
        private readonly MessageFactory _messageFactory;
        private readonly MessageSerializer _serializer = new MessageSerializer();

        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private string _domainName;
        private bool _isStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportLayer"/> class.
        /// </summary>
        public TransportLayer(MessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
            _messageFactory.RequestReceived += OnRequest;
            _messageFactory.ResponseReceived += OnResponse;
        }

        /// <summary>
        /// Create a endpoint for the destination that should be reached.
        /// </summary>
        /// <param name="request">Request to send</param>
        /// <returns>Endpoint if destination could be looked up; otherwise <c>null</c>.</returns>
        protected virtual EndPoint CreateEndPoint(IRequest request)
        {
            string domain = request.Uri.Domain;
            IPHostEntry entry = Dns.GetHostEntry(domain);
            if (entry.AddressList.Length == 0)
                return null;

            int port = request.Uri.Scheme == "sips" ? 5061 : 5060;
            return new IPEndPoint(entry.AddressList[0], port);
        }

        private string GetMessage(IRequest request)
        {
            byte[] buffer = _buffers.Dequeue();
            long count = _serializer.Serialize(request, buffer);
            return Encoding.ASCII.GetString(buffer, 0, (int) count);
        }

        private string GetMessage(IResponse response)
        {
            byte[] buffer = _buffers.Dequeue();
            long count = _serializer.Serialize(response, buffer);
            return Encoding.ASCII.GetString(buffer, 0, (int) count);
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            /* RFC 3581 4. 
                When a server compliant to this specification (which can be a proxy
                or UAS) receives a request, it examines the topmost Via header field
                value.  If this Via header field value contains an "rport" parameter
                with no value, it MUST set the value of the parameter to the source
                port of the request.  This is analogous to the way in which a server
                will insert the "received" parameter into the topmost Via header
                field value.  In fact, the server MUST insert a "received" parameter
                containing the source IP address that the request came from, even if
                it is identical to the value of the "sent-by" component.  Note that
                this processing takes place independent of the transport protocol.
            */

            ViaEntry via = e.Request.Via.First;
            var ep = e.RemoteEndPoint as IPEndPoint;
            if (ep != null)
            {
                via.Received = ep.Address.ToString();
                if (via.RportWanted)
                    via.Rport = ep.Port;
            }


            /* RFC 3261 18.2.1.
                When the server transport receives a request over any transport, it
                MUST examine the value of the "sent-by" parameter in the top Via
                header field value.  If the host portion of the "sent-by" parameter
                contains a domain name, or if it contains an IP address that differs
                from the packet source address, the server MUST add a "received"
                parameter to that Via header field value.  This parameter MUST
                contain the source address from which the packet was received.  This
                is to assist the server transport layer in sending the response,
                since it must be sent to the source IP address from which the request
                came.

                Next, the server transport attempts to match the request to a server
                transaction.  It does so using the matching rules described in
                Section 17.2.3.  If a matching server transaction is found, the
                request is passed to that transaction for processing.  If no match is
                found, the request is passed to the core, which may decide to
                construct a new server transaction for that request.  Note that when
                a UAS core sends a 2xx response to INVITE, the server transaction is
                destroyed.  This means that when the ACK arrives, there will be no
                matching server transaction, and based on this rule, the ACK is
                passed to the UAS core, where it is processed.
            */
            //_logger.Debug(GetMessage(e.Request));
            RequestReceived(this, e);
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            /* 3261 18.1.2
               When a response is received, the client transport examines the top
               Via header field value.  If the value of the "sent-by" parameter in
               that header field value does not correspond to a value that the
               client transport is configured to insert into requests, the response
               MUST be silently discarded.
             */
            //_logger.Debug(GetMessage(e.Response));
            ResponseReceived(this, e);
        }

        /// <summary>
        /// Register a new transport implementation.
        /// </summary>
        /// <param name="transport"></param>
        /// <exception cref="InvalidOperationException">Server have already been started.</exception>
        public void Register(ITransport transport)
        {
            if (_isStarted)
                throw new InvalidOperationException("Server have already been started.");

            _transports.Add(transport.Protocol, transport);
            transport.BufferPool = _buffers;
        }


        /// <summary>
        /// Start layer.
        /// </summary>
        public void Start()
        {
            _isStarted = true;
        }

        #region ITransportLayer Members

        public void Send(IRequest request)
        {
            /*
                Before a request is sent, the client transport MUST insert a value of
                the "sent-by" field into the Via header field.  This field contains
                an IP address or host name, and port.  The usage of an FQDN is
                RECOMMENDED.  This field is used for sending responses under certain
                conditions, described below.  If the port is absent, the default
                value depends on the transport.  It is 5060 for UDP, TCP and SCTP,
                5061 for TLS.
             */

            ITransport transport = _transports[request.Via.First.Transport];
            request.Via.First.SentBy = _domainName + ":" + transport.Port;

            /*
                If a request is within 200 bytes of the path MTU, or if it is larger
                than 1300 bytes and the path MTU is unknown, the request MUST be sent
                using an RFC 2914 [43] congestion controlled transport protocol, such
                as TCP. If this causes a change in the transport protocol from the
                one indicated in the top Via, the value in the top Via MUST be
                changed.  This prevents fragmentation of messages over UDP and
                provides congestion control for larger messages.  However,
                implementations MUST be able to handle messages up to the maximum
                datagram packet size.  For UDP, this size is 65,535 bytes, including
                IP and UDP headers.
            */

            byte[] buffer = _buffers.Dequeue();
            long count = _serializer.Serialize(request, buffer);

            if (count > 65335)
            {
                //Serialize again with new sent-by header.
                transport = _transports["TCP"];
                request.Via.First.SentBy = _domainName + ":" + transport.Port;
                count = _serializer.Serialize(request, buffer);
            }

            // Create end point and send.
            EndPoint ep = CreateEndPoint(request);
            if (ep == null)
            {
                _logger.Warning("Failed to find destination for " + request);
                return;
            }

            // And send it.
            transport.Send(ep, buffer, 0, (int) count);
        }


        public void Send(IResponse response)
        {
            /*
               The server transport uses the value of the top Via header field in
               order to determine where to send a response.  It MUST follow the
               following process:

                  o  If the "sent-protocol" is a reliable transport protocol such as
                     TCP or SCTP, or TLS over those, the response MUST be sent using
                     the existing connection to the source of the original request
                     that created the transaction, if that connection is still open.
                     This requires the server transport to maintain an association
                     between server transactions and transport connections.  If that
                     connection is no longer open, the server SHOULD open a
                     connection to the IP address in the "received" parameter, if
                     present, using the port in the "sent-by" value, or the default
                     port for that transport, if no port is specified.  If that
                     connection attempt fails, the server SHOULD use the procedures
                     in [4] for servers in order to determine the IP address and
                     port to open the connection and send the response to.

                  o  Otherwise, if the Via header field value contains a "maddr"
                     parameter, the response MUST be forwarded to the address listed
                     there, using the port indicated in "sent-by", or port 5060 if
                     none is present.  If the address is a multicast address, the
                     response SHOULD be sent using the TTL indicated in the "ttl"
                     parameter, or with a TTL of 1 if that parameter is not present.

                  o  Otherwise (for unreliable unicast transports), if the top Via
                     has a "received" parameter, the response MUST be sent to the
                     address in the "received" parameter, using the port indicated
                     in the "sent-by" value, or using port 5060 if none is specified
                     explicitly.  If this fails, for example, elicits an ICMP "port
                     unreachable" response, the procedures of Section 5 of [4]
                     SHOULD be used to determine where to send the response.
            */

            ViaEntry via = response.Via.First;
            if (string.IsNullOrEmpty(via.Received))
            {
                _logger.Warning("Received was not specified in " + response);
                return;
            }

            string targetDomain;
            int port = 5060;
            if (!string.IsNullOrEmpty(via.SentBy))
            {
                targetDomain = via.SentBy;
                int index = via.SentBy.IndexOf(':');
                if (index > 0 && index < via.SentBy.Length - 1)
                {
                    string temp = via.SentBy.Substring(index + 1);
                    if (!int.TryParse(temp, out port))
                        port = 5060;
                }
            }
            else
                targetDomain = via.Received;

            // RFC3841
            if (via.Rport > 0)
                port = via.Rport;

            IPHostEntry entry = Dns.GetHostEntry(targetDomain);
            if (entry.AddressList.Length == 0)
            {
                _logger.Warning("Failed to find host entry for: " + via.Received);
                return;
            }
            EndPoint ep = new IPEndPoint(entry.AddressList[0], port);


            byte[] buffer = _buffers.Dequeue();
            int count = _serializer.Serialize(response, buffer);
            _transports[via.Transport].Send(ep, buffer, 0, count);
        }

        #endregion

        public event EventHandler<ResponseEventArgs> ResponseReceived;
        public event EventHandler<RequestEventArgs> RequestReceived;
    }
}