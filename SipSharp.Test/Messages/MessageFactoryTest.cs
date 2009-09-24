using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using Xunit;

namespace SipSharp.Test.Messages
{
    public class MessageFactoryTest
    {
        private MessageFactory _factory;
        private HeaderFactory _headerFactory;
        private IRequest _request;
        private IResponse _response;

        public MessageFactoryTest()
        {
            LogFactory.Assign(ConsoleLogFactory.Instance);

            _headerFactory = new HeaderFactory(new StringHeader("Prototype"));
            _headerFactory.AddDefaultParsers();
            _factory = new MessageFactory(_headerFactory);
            _factory.RequestReceived += OnRequest;
            _factory.ResponseReceived += OnResponse;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            _response = e.Response;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            _request = e.Request;
        }

        [Fact]
        private void TestFactory()
        {
            MessageFactoryContext context = _factory.CreateNewContext();
            Parse(context, Messages.AShortTortuousINVITE);
            Assert.NotNull(_request);
            Assert.Equal(_request.Uri.Domain, "chair-dnrc.example.com");
        }

        private void Parse(MessageFactoryContext context, string message)
        {
            Parse(context, message, 0);
        }

        private void Parse(MessageFactoryContext context, string message, int offset)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            if (offset == 0)
                context.Parse(bytes, 0, bytes.Length);
            else
            {
                context.Parse(bytes, 0, offset);
                context.Parse(bytes, offset, bytes.Length - offset);
            }
        }
    }
}
