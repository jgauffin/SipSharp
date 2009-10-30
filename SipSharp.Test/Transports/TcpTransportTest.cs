using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Tools;
using SipSharp.Transports;
using Xunit;

namespace SipSharp.Test.Transports
{
    public class TcpTransportTest
    {
        private TcpTransport _transport;
        private MessageFactory _messageFactory;
        private IResponse _response;
        private IRequest _request;
        private ManualResetEvent _manualEvent = new ManualResetEvent(false);
        private ObjectPool<byte[]> _bufferPool = new ObjectPool<byte[]>(() => new byte[4096]);
        private byte[] _recievedBuffer;
        private int _receivedLength;


        public TcpTransportTest()
        {
            LogFactory.Assign(new ConsoleLogFactory(null));

            HeaderFactory headerFactory = new HeaderFactory();
            headerFactory.AddDefaultParsers();
            _messageFactory = new MessageFactory(headerFactory);
            _messageFactory.RequestReceived += OnRequest;
            _messageFactory.ResponseReceived += OnResponse;
            ObjectPool<byte[]> pool = new ObjectPool<byte[]>(CreateBuffer);
            _transport = new TcpTransport(_messageFactory) {BufferPool = _bufferPool};
            _transport.Start(new IPEndPoint(IPAddress.Any, 1324));
            _transport.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Assert.False(true);
        }

        [Fact]
        private void ReceiveRequest()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, 1324);

            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0")
                           {
                               From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060)),
                               To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com")),
                               CallId = Guid.NewGuid().ToString().Replace("-", string.Empty),
                               CSeq = new CSeq(1, _request.Method),
                               MaxForwards = 60
                           };
            _request.Contact = _request.To;


            byte[] buffer =_bufferPool.Dequeue();
            MessageSerializer serializer = new MessageSerializer();
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

            TcpListener listener = new TcpListener(IPAddress.Loopback, 12721);
            listener.Start();
            listener.BeginAcceptSocket(OnNewSocket, listener);

            byte[] buffer =_bufferPool.Dequeue();
            MessageSerializer serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, 12721), buffer, 0, length);

            Assert.True(_manualEvent.WaitOne(1000));

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

            TcpListener listener = new TcpListener(IPAddress.Loopback, 12721);
            listener.Start();
            listener.BeginAcceptSocket(OnNewSocket, listener);

            byte[] buffer = _bufferPool.Dequeue();
            MessageSerializer serializer = new MessageSerializer();
            int length = serializer.Serialize(_response, buffer);
            _transport.Send(new IPEndPoint(IPAddress.Loopback, 12721), buffer, 0, length);

            Assert.True(_manualEvent.WaitOne(1000));
        }

        private void OnNewSocket(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener) ar.AsyncState;
            Socket socket = listener.EndAcceptSocket(ar);

            _recievedBuffer = new byte[65535];
            _receivedLength = socket.Receive(_recievedBuffer, 0, 65535, SocketFlags.None);
            _manualEvent.Set();
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            throw new NotImplementedException();
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

        private byte[] CreateBuffer()
        {
            return new byte[4196];
        }
    }
}
