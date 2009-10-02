using System;
using SipSharp.Messages.Headers;
using SipSharp.Tools;

namespace SipSharp.Messages
{
    /// <summary>
    /// Parses and builds messages
    /// </summary>
    /// <remarks>
    /// <para>The message factory takes care of building messages
    /// from all end points.</para>
    /// <para>
    /// Since both message and packet protocols are used, the factory 
    /// hands out contexts to all end points. The context keeps a state
    /// to be able to parse partial messages propertly.
    /// </para>
    /// <para>
    /// Each end point need to hand the context back to the message factory
    /// when the client disconnects (or a message have been parsed).
    /// </para>
    /// </remarks>
    internal class MessageFactory
    {
        private readonly HeaderFactory _factory;
        private readonly ObjectPool<MessageFactoryContext> _builders;

        public MessageFactory(HeaderFactory factory)
        {
            _factory = factory;
            _builders = new ObjectPool<MessageFactoryContext>(CreateBuilder);
        }

        private MessageFactoryContext CreateBuilder()
        {
            MessageFactoryContext mb = new MessageFactoryContext(this, _factory, new SipParser());
            mb.RequestCompleted += OnRequest;
            mb.ResponseCompleted += OnResponse;
            return mb;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            ResponseReceived(this, e);
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            RequestReceived(this, e);
        }

        public Request CreateRequest(string method, string path, string version)
        {
            return new Request(method, path, version);
        }

        public Response CreateResponse(string version, StatusCode statusCode, string reason)
        {
            return new Response(version, statusCode, reason);
        }

        public MessageFactoryContext CreateNewContext()
        {
            return _builders.Dequeue();
        }

        /// <summary>
        /// Release a used factoryContext.
        /// </summary>
        /// <param name="factoryContext"></param>
        public void Release(MessageFactoryContext factoryContext)
        {
            _builders.Enqueue(factoryContext);
        }

        /// <summary>
        /// A request have been received from one of the end points.
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestReceived = delegate { };

        /// <summary>
        /// A response have been receivied from one of the end points.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };
    }
}
