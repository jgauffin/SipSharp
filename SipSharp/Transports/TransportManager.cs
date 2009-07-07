using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SipSharp.Transports.Parser;

namespace SipSharp.Transports
{
    public interface ITransportManager
    {
        /// <summary>
        /// Send a request to the remote end.
        /// </summary>
        /// <param name="remoteEndPoint">Remote end point.</param>
        /// <param name="request">Request to send.</param>
        void Send(EndPoint remoteEndPoint, IRequest request);

        /// <summary>
        /// A response have been received from an end point.
        /// </summary>
        event EventHandler<ResponseEventArgs> ResponseReceived;

        /// <summary>
        /// A request have been received from an end point.
        /// </summary>
        event EventHandler<RequestEventArgs> RequestReceived;
    }

    /// <summary>
    /// Takes care of transportation of messages.
    /// </summary>
    public class TransportManager : ITransportManager
    {
        readonly MessageSerializer _serializer = new MessageSerializer();
        readonly Queue<Stream> _serializerStreams = new Queue<Stream>();
        public const int UdpMaxSize = 65507;
        private readonly ParserManager _parserManager;
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private bool _isStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportManager"/> class.
        /// </summary>
        public TransportManager()
        {
            _parserManager = new ParserManager();
            _parserManager.RequestReceived += OnRequest;
            _parserManager.ResponseReceived += OnResponse;
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

        private byte[] Serialize(IRequest request)
        {
            Stream stream = GetStream();

            _serializer.Serialize(request, stream);
            stream.Seek(0, SeekOrigin.Begin);
            if (stream.Length > UdpMaxSize)
            {
                
            }
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            lock (_serializerStreams)
                _serializerStreams.Enqueue(stream);
            return buffer;
        }

        private Stream GetStream()
        {
            lock (_serializerStreams)
            {
                if (_serializerStreams.Count > 0)
                    return _serializerStreams.Dequeue();
            }

            return new MemoryStream();
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            ResponseReceived(this, e);
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            RequestReceived(this, e);
        }




        /// <summary>
        /// A response have been received from an end point.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate{};

        /// <summary>
        /// A request have been received from an end point.
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestReceived = delegate { };

    	public void Send(EndPoint point, IResponse response)
    	{
    		
    	}
    }
}
