using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SipSharp.Headers;
using SipSharp.Messages;
using SipSharp.Tools;
using SipSharp.Transports.Parser;

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
        readonly MessageSerializer _serializer = new MessageSerializer();
        private readonly ObjectPool<Stream> _serializerStreams = new ObjectPool<Stream>(() => new MemoryStream());
        public const int UdpMaxSize = 65507;
        private readonly MessageFactory _messageFactory;
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
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

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            /* 3261 18.1.2
               When a response is received, the client transport examines the top
               Via header field value.  If the value of the "sent-by" parameter in
               that header field value does not correspond to a value that the
               client transport is configured to insert into requests, the response
               MUST be silently discarded.
             */
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {

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
            IPEndPoint ep = e.RemoteEndPoint as IPEndPoint;
            if (ep != null)
            {
                via.Received = ep.Address.ToString();
                if (via.RportWanted)
                    via.Rport = ep.Port;
            }

        }

        /// <summary>
        /// Start layer.
        /// </summary>
        public void Start()
        {
            _isStarted = true;
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
        }

        /// <summary>
        /// Send a request to the remote end.
        /// </summary>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <param name="request">Request to send.</param>
        public void Send(EndPoint remoteEndPoint, IRequest request)
        {
            byte[] buffer = Serialize(request);
            _transports["udp"].Send(remoteEndPoint, buffer, 0, buffer.Length);
        }

        public void Send(IRequest request)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<ResponseEventArgs> ResponseReceived;
        public event EventHandler<RequestEventArgs> RequestReceived;

        public void Send(IResponse response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert a package to bytes and send it.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private byte[] Serialize(IRequest request)
        {
            Stream stream = _serializerStreams.Dequeue();

            _serializer.Serialize(request, stream);
            stream.Seek(0, SeekOrigin.Begin);
            if (stream.Length > UdpMaxSize)
            {
                //TODO: Switch to TCP
            }
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            _serializerStreams.Enqueue(stream);
            return buffer;
        }


    	public void Send(EndPoint point, IResponse response)
    	{
    		
    	}
    }
}
