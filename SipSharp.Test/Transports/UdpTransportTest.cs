using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Tools;
using SipSharp.Transports;
using Xunit;

namespace SipSharp.Test.Transports
{
    public class UdpTransportTest
    {
        private const int PortNumber = 5032;
        private readonly ObjectPool<byte[]> _bufferPool = new ObjectPool<byte[]>(() => new byte[4096]);
        private readonly ManualResetEvent _manualEvent = new ManualResetEvent(false);
        private readonly MessageFactory _messageFactory;
        private readonly UdpTransport _transport;
        private int _receivedLength;
        private byte[] _recievedBuffer;
        private IRequest _request;
        private IResponse _response;
        private Socket _testSocket;

        public UdpTransportTest()
        {
            LogFactory.Assign(new ConsoleLogFactory(null));

            var headerFactory = new HeaderFactory();
            headerFactory.AddDefaultParsers();
            _messageFactory = new MessageFactory(headerFactory);
            _messageFactory.RequestReceived += OnRequest;
            _messageFactory.ResponseReceived += OnResponse;
            var pool = new ObjectPool<byte[]>(CreateBuffer);
            _transport = new UdpTransport(_messageFactory) {BufferPool = _bufferPool};
            _transport.Start(new IPEndPoint(IPAddress.Loopback, PortNumber));
            _transport.UnhandledException += OnUnhandledException;
        }

        private byte[] CreateBuffer()
        {
            return new byte[4196];
        }

        private IAsyncResult CreateListener()
        {
            _testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _testSocket.Bind(new IPEndPoint(IPAddress.Any, 12721));
            byte[] inbuffer = _bufferPool.Dequeue();
            EndPoint ep = new IPEndPoint(IPAddress.Any, 1234);
            return _testSocket.BeginReceiveFrom(inbuffer, 0, inbuffer.Length, SocketFlags.None, ref ep, null, null);
        }

        private void OnNewSocket(IAsyncResult ar)
        {
            var listener = (TcpListener) ar.AsyncState;
            Socket socket = listener.EndAcceptSocket(ar);

            _recievedBuffer = new byte[65535];
            _receivedLength = socket.Receive(_recievedBuffer, 0, 65535, SocketFlags.None);
            _manualEvent.Set();
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            Assert.Equal(_request.To, e.Request.To);
            Assert.Equal(_request.From, e.Request.From);
            Assert.Equal(_request.Contact, e.Request.Contact);
            Assert.Equal(_request.CallId, e.Request.CallId);
            Assert.Equal(_request.CSeq, e.Request.CSeq);
            Assert.Equal(_request.MaxForwards, e.Request.MaxForwards);
            _manualEvent.Set();
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Assert.False(true);
        }

        [Fact]
        private void ReceiveRequest()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(IPAddress.Loopback, PortNumber);

            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0")
                           {
                               From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                               To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                               CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                               MaxForwards = 60
                           };
            _request.CSeq = new CSeq(1, _request.Method);
            _request.Contact = _request.To;


            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            socket.Send(buffer, 0, length, SocketFlags.None);
            Assert.True(_manualEvent.WaitOne(1000));
        }

        [Fact]
        private void SendRequest()
        {
            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0")
                           {
                               From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                               To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                               CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                               MaxForwards = 60,
                           };
            _request.CSeq = new CSeq(1, _request.Method);
            _request.Contact = _request.To;

            IAsyncResult res = CreateListener();

            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, 12721), buffer, 0, length);

            EndPoint ep = new IPEndPoint(IPAddress.Any, 1234);
            int bytesRead = _testSocket.EndReceiveFrom(res, ref ep);
            Assert.NotEqual(0, bytesRead);
        }

        [Fact]
        private void SendResponse()
        {
            _response = new Response("SIP/2.0", StatusCode.OK, "OK!")
                            {
                                From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                                To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                                CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                            };
            _response.CSeq = new CSeq(1, "INVITE");
            //_response.Contact = _request.To;

            IAsyncResult res = CreateListener();

            byte[] buffer = _bufferPool.Dequeue();
            var serializer = new MessageSerializer();
            int length = serializer.Serialize(_response, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, 12721), buffer, 0, length);

            EndPoint ep = new IPEndPoint(IPAddress.Any, 1234);
            int bytesRead = _testSocket.EndReceiveFrom(res, ref ep);
            Assert.NotEqual(0, bytesRead);
        }
    }
}