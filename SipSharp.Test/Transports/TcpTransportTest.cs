using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public TcpTransportTest()
        {
            HeaderFactory headerFactory = new HeaderFactory(new StringHeader("Generic"));
            headerFactory.AddDefaultParsers();
            _messageFactory = new MessageFactory(headerFactory);
            _messageFactory.RequestReceived += OnRequest;
            _messageFactory.ResponseReceived += OnResponse;
            ObjectPool<byte[]> pool = new ObjectPool<byte[]>(CreateBuffer);
            _transport = new TcpTransport(_messageFactory, pool);
            _transport.Start(new IPEndPoint(IPAddress.Any, 1324));
        }

        [Fact]
        private void SendMessage()
        {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Loopback, 1324);

            _request = new Request("INVITE", "sips:jonas@gauffin.com", "SIP/2.0");
            MessageSerializer serializer = new MessageSerializer();
            serializer.Serialize(_request, client.GetStream());
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            
        }

        private byte[] CreateBuffer()
        {
            return new byte[4196];
        }
    }
}
