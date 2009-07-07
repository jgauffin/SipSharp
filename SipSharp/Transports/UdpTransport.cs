using System;
using System.Net;
using System.Net.Sockets;
using SipSharp.Transports.Parser;

namespace SipSharp.Transports
{
    internal class UdpTransport : ITransport
    {
        private readonly ILogWriter _logWriter = LogFactory.CreateLogger(typeof (UdpTransport));
        private Socket _socket;
        private readonly BufferPool _bufferPool;
        private readonly ParserManager _parsers;
        private EndPoint _serverEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpTransport"/> class.
        /// </summary>
        /// <param name="bufferPool">The buffer pool.</param>
        /// <param name="parsers">The parsers.</param>
        public UdpTransport(BufferPool bufferPool, ParserManager parsers)
        {
            _bufferPool = bufferPool;
            _parsers = parsers;
        }

        protected virtual Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void OnRead(IAsyncResult ar)
        {
            var buffer = (byte[]) ar.AsyncState;
            EndPoint endPoint = null;
            int bytesRead = _socket.EndReceiveFrom(ar, ref endPoint);

            // begin receiving another packet before starting to process this one
            byte[] newBuffer = _bufferPool.Dequeue();
            _socket.BeginReceiveFrom(newBuffer, 0, newBuffer.Length, SocketFlags.None,
                                            ref _serverEndPoint, OnRead, newBuffer);

            // and parse current packet.
            _parsers.Parse(endPoint, buffer, 0, bytesRead);
            _bufferPool.Enqueue(buffer);
        }

        

        private void OnSendComplete(IAsyncResult ar)
        {
            var context = (SendContext) ar.AsyncState;
            int bytesSent = _socket.EndSendTo(ar);
            if (context.buffer.Length != bytesSent)
            {
                _logWriter.Write(this, LogLevel.Warning, "Failed to send whole UDP message, " + bytesSent +
                                                         " of " + context.buffer.Length + " bytes to " + context.endPoint);
            }
            _bufferPool.Enqueue(context.buffer);
        }


        #region Nested type: ClientContext

        private struct SendContext
        {
            public readonly EndPoint endPoint;
            public readonly byte[] buffer;

            public SendContext(EndPoint endPoint, byte[] buffer)
            {
                this.endPoint = endPoint;
                this.buffer = buffer;
            }
        }

        #endregion

        /// <summary>
        /// Start transport.
        /// </summary>
        /// <param name="listenerEndPoint">Address/port that clients should connect to.</param>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        public void Start(EndPoint listenerEndPoint)
        {
            if (listenerEndPoint == null)
                throw new ArgumentNullException("listenerEndPoint");
            IPEndPoint ep = listenerEndPoint as IPEndPoint;
            if (ep == null)
                throw new ArgumentException("Endpoint is not of type IPEndPoint", "listenerEndPoint");

            byte[] buffer = _bufferPool.Dequeue();
            _socket = CreateSocket();
            _serverEndPoint = listenerEndPoint;
            _socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref _serverEndPoint, OnRead, buffer);
        }

        public void Send(EndPoint endPoint, byte[] buffer, int offset, int count)
        {
            SendContext context = new SendContext(endPoint, buffer);
            _socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, endPoint, OnSendComplete,
                                       context);
        }

        /// <summary>
        /// Gets protocol used by this transporter.
        /// </summary>
        public string Protocol
        {
            get { return "UDP"; }
        }
    }
}