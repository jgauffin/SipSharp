using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SipSharp.Messages;
using SipSharp.Tools;

namespace SipSharp.Transports
{
    /// <summary>
    /// Transports messages over TCP.
    /// </summary>
    /// <remarks>
    /// This transport takes care of ALL sockets for TCP.
    /// </remarks>
    internal class TcpTransport : ITransport
    {
        private readonly Dictionary<EndPoint, ClientContext> _contexts = new Dictionary<EndPoint, ClientContext>();
        private readonly MessageFactory _factory;
        private TcpListener _listener;
        private BufferReader _reader;
        private IPEndPoint _listeningPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransport"/> class.
        /// </summary>
        /// <param name="endPoint">End point to accept new connections on.</param>
        /// <param name="factory">Message factory.</param>
        public TcpTransport(IPEndPoint endPoint, MessageFactory factory)
        {
            _listeningPoint = endPoint;
            _factory = factory;
        }

        /// <summary>
        /// Create a new socket and try to connect to a remote client/server.
        /// </summary>
        /// <param name="endPoint">Endpoint that should get connected.</param>
        /// <returns>Connected client.</returns>
        /// <exception cref="SocketException">If connection fails.</exception>
        protected virtual ClientContext CreateAndConnect(IPEndPoint endPoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socket.Connect(endPoint);
            return new ClientContext(socket, BufferPool.Dequeue(), _factory.CreateNewContext(endPoint));
        }

        private void HandleDisconnect(ClientContext context)
        {
            BufferPool.Enqueue(context.Buffer);
            _factory.Release(context.Parser);
            context.Socket.Disconnect(true);
        }

        private void OnAccept(IAsyncResult ar)
        {
            _listener.BeginAcceptSocket(OnAccept, null);
            Socket socket = _listener.EndAcceptSocket(ar);
            SetupNewContext(socket);
        }

        private void OnReceive(IAsyncResult ar)
        {
            var context = (ClientContext) ar.AsyncState;
            int bytesRead = context.Socket.EndReceive(ar);
            if (bytesRead == 0)
            {
                HandleDisconnect(context);
                return;
            }

            try
            {
                int offset = context.Parser.Parse(context.Buffer, context.Offset, context.Offset + bytesRead);

                // stuff parsed? then try again until nothing more is parsed.
                if (offset != context.Offset)
                {
                    while (offset != context.Offset)
                        offset = context.Parser.Parse(context.Buffer, context.Offset, context.Offset + bytesRead);
                }

                context.Offset = offset;
                context.Socket.BeginReceive(context.Buffer, context.Offset, context.Buffer.Length - context.Offset,
                                            SocketFlags.None, OnReceive, context);
            }
            catch (SipException err)
            {
            }
            catch (Exception err)
            {
                UnhandledException(this, new UnhandledExceptionEventArgs(err, false));
            }
        }

        private void OnSend(IAsyncResult ar)
        {
        }

        /// <summary>
        /// Create a new context and allocate it's buffer.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <exception cref="InvalidOperationException">Could not allocate new buffer.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">Could not read from socket.</exception>
        /// <exception cref="System.ObjectDisposedException">Socket have been disposed.</exception>
        protected virtual void SetupNewContext(Socket socket)
        {
            var context = new ClientContext(socket, BufferPool.Dequeue(),
                                            _factory.CreateNewContext(socket.RemoteEndPoint));
            if (context.Buffer == null)
                throw new InvalidOperationException("Could not allocate new buffer.");

            socket.BeginReceive(context.Buffer, 0, context.Buffer.Length, SocketFlags.None, OnReceive, context);
        }

        #region ITransport Members

        /// <summary>
        /// Starts the specified end point.
        /// </summary>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        /// <exception cref="System.Net.Sockets.SocketException">If listener cannot be started.</exception>
        public void Start()
        {
            _listener = new TcpListener(_listeningPoint);
            _listener.Start();
            _listener.BeginAcceptSocket(OnAccept, null);
        }


        /// <summary>
        /// Gets or sets listening end point.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get { return _listeningPoint; }
        }

        /// <summary>
        /// Send a buffer to a certain end point.
        /// </summary>
        /// <param name="endPoint">End point that the buffer should be sent to.</param>
        /// <param name="buffer">buffer to send.</param>
        /// <param name="offset">Position of first byte to send.</param>
        /// <param name="count">Number of bytes, from offset, to send.</param>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><c>offset+count</c> is out of range.</exception>
        public void Send(EndPoint endPoint, byte[] buffer, int offset, int count)
        {
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length < offset + count)
                throw new ArgumentOutOfRangeException("offset", "Offset+count is larger than buffers length.");
            var ep = endPoint as IPEndPoint;
            if (ep == null)
                throw new ArgumentException("Endpoint is not a IPEndPoint.");

            ClientContext context;
            bool isCreated;
            lock (_contexts)
                isCreated = _contexts.TryGetValue(endPoint, out context);

            // since we dont want to lock _contexts during connection tries.
            if (!isCreated)
            {
                context = CreateAndConnect(ep);
                lock (_contexts)
                {
                    if (!_contexts.ContainsKey(endPoint))
                        _contexts.Add(endPoint, context);
                }
            }

            context.Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, OnSend, null);
        }

        /// <summary>
        /// Gets protocol used by this transporter.
        /// </summary>
        public string Protocol
        {
            get { return "TCP"; }
        }

        /// <summary>
        /// Gets port that the point is listening on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets of protocol is message based.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Message based protocols like UDP should only receive one (and a complete) message
        /// in each receive. While packet based protocols like TCP can receive partial, complete or multiple
        /// messages in one packet.
        /// </para>
        /// <para>This property should be used to </para>
        /// </remarks>
        //string IsMessageBasedProtocl{ get;}
        public ObjectPool<byte[]> BufferPool { set; private get; }

        /// <summary>
        /// A exception that was not handled by a worker thread.
        /// </summary>
        public event UnhandledExceptionEventHandler UnhandledException = delegate { };

        #endregion

        #region Nested type: ClientContext

        protected struct ClientContext
        {
            public ClientContext(Socket socket, byte[] buffer, MessageFactoryContext parser)
                : this()
            {
                Socket = socket;
                Parser = parser;
                Buffer = buffer;
                Offset = 0;
            }

            public byte[] Buffer { get; private set; }

            public int Offset { get; set; }
            public MessageFactoryContext Parser { get; private set; }
            public Socket Socket { get; private set; }
        }

        #endregion
    }
}