using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private IRequest _request;
        private ManualResetEvent _requestEvent = new ManualResetEvent(false);
        private ObjectPool<byte[]> _bufferPool = new ObjectPool<byte[]>(() => new byte[4096]);

        public TcpTransportTest()
        {
            HeaderFactory headerFactory = new HeaderFactory(new StringHeader("Generic"));
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
        private void SendMessage()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Loopback, 1324);

            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0");
            _request.From = new Contact("Adam Nilsson", new SipUri("sip", "adam", "nilsson.com", 5060));
            _request.To = new Contact("Jonas Gauffin", new SipUri("jonas", "gauffin.com"));
            _request.Contact = _request.To;
            _request.CallId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            _request.CSeq = new CSeq(1, _request.Method);
            _request.MaxForwards = 60;


            byte[] buffer =_bufferPool.Dequeue();
            MessageSerializer serializer = new MessageSerializer();
            int length = serializer.Serialize(_request, buffer);
            socket.Send(buffer, 0, length, SocketFlags.None);
            Assert.True(_requestEvent.WaitOne(1000));

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
            _requestEvent.Set();
        }

        private byte[] CreateBuffer()
        {
            return new byte[4196];
        }
    }
}
