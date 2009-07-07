using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SipSharp.Transports.Parser;

namespace SipSharp.Transports
{
    /// <summary>
    /// Transports messages over TCP.
    /// </summary>
    class TcpTransport : ITransport
    {
        private TcpListener _listener;
        private readonly BufferPool _bufferPool;
        private readonly ParserManager _parsers;
        private readonly Dictionary<EndPoint, ClientContext> _contexts = new Dictionary<EndPoint, ClientContext>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTransport"/> class.
        /// </summary>
        /// <param name="parsers">Parser manager, used to get parsers for messages.</param>
        /// <param name="bufferPool">Used to retrieve buffers for receiving messages.</param>
        public TcpTransport(ParserManager parsers, BufferPool bufferPool)
        {
            _parsers = parsers;
            _bufferPool = bufferPool;
        }

    	/// <summary>
    	/// Starts the specified end point.
    	/// </summary>
    	/// <param name="endPoint">The end point.</param>
    	/// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
    	/// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
    	/// <exception cref="System.Net.Sockets.SocketException">If listener cannot be started.</exception>
        public void Start(EndPoint endPoint)
        {
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            IPEndPoint ep = endPoint as IPEndPoint;
            if (ep == null)
                throw new ArgumentException("EndPoint is not of type IPEndPoint");

            _listener = new TcpListener(ep);
            _listener.Start();
            _listener.BeginAcceptSocket(OnAccept, null);
        }

        /// <summary>
        /// Create a new socket and try to connect to a remote client/server.
        /// </summary>
        /// <param name="endPoint">Endpoint that should get connected.</param>
        /// <returns>Connected client.</returns>
        /// <exception cref="SocketException">If connection fails.</exception>
        protected virtual ClientContext CreateAndConnect(IPEndPoint endPoint)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            socket.Connect(endPoint);
            return new ClientContext(socket, _bufferPool.Dequeue(), _parsers.Dequeue());
        }

        private void OnSend(IAsyncResult ar)
        {
            
        }

        private void OnAccept(IAsyncResult ar)
        {
            _listener.BeginAcceptSocket(OnAccept, null);
            Socket socket = _listener.EndAcceptSocket(ar);
            SetupNewContext(socket);
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
            ClientContext context = new ClientContext(socket, _bufferPool.Dequeue(), _parsers.Dequeue());
			if (context.Buffer == null)
				throw new InvalidOperationException("Could not allocate new buffer.");

            socket.BeginReceive(context.Buffer, 0, context.Buffer.Length, SocketFlags.None, OnReceive, context);
        }

        private void OnReceive(IAsyncResult ar)
        {
            ClientContext context = (ClientContext) ar.AsyncState;
            int bytesRead = context.Socket.EndReceive(ar);
            if (bytesRead == 0)
            {
                HandleDisconnect(context);
                return;
            }

			try
			{
				context.Offset = context.Parser.Parse(context.Buffer, 0, context.Offset + bytesRead);
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


        private void HandleDisconnect(ClientContext context)
        {
            _parsers.Enqueue(context.Parser);
            _bufferPool.Enqueue(context.Buffer);
            context.Socket.Disconnect(true);
        }

        protected struct ClientContext
        {
            public ClientContext(Socket socket, byte[] buffer, IMessageParser parser) : this()
            {
                Socket = socket;
                Parser = parser;
                Buffer = buffer;
                Offset = 0;
            }

            public byte[] Buffer { get; private set; }

            public Socket Socket { get; private set; }

            public IMessageParser Parser { get; private set; }

            public int Offset { get; set; }
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
            IPEndPoint ep = endPoint as IPEndPoint;
            if (ep == null)
                throw new ArgumentException("Endpoint is not a IPEndPoint.");

            ClientContext context;
            bool created;
            lock (_contexts)
            {
                created = !_contexts.TryGetValue(endPoint, out context);
            }

            // since we dont want to look _contexts during connection tries.
            if (created)
            {
                context = CreateAndConnect(ep);
                lock (_contexts)
                    _contexts.Add(endPoint, context);
                
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
		/// A exception that was not handled by a worker thread.
		/// </summary>
    	public event UnhandledExceptionEventHandler UnhandledException = delegate{};

		
    }
}
