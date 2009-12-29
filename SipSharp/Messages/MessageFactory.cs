using System;
using System.Net;
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
    /// to be able to parse partial messages properly.
    /// </para>
    /// <para>
    /// Each end point need to hand the context back to the message factory
    /// when the client disconnects (or a message have been parsed).
    /// </para>
    /// </remarks>
    public class MessageFactory
    {
        private readonly ObjectPool<MessageFactoryContext> _builders;
        private readonly HeaderFactory _factory;

        public MessageFactory(HeaderFactory factory)
        {
            _factory = factory;
            _builders = new ObjectPool<MessageFactoryContext>(CreateBuilder);
        }

        private MessageFactoryContext CreateBuilder()
        {
            var mb = new MessageFactoryContext(this, _factory, new SipParser());
            mb.RequestCompleted += OnRequest;
            mb.ResponseCompleted += OnResponse;
            return mb;
        }

        /// <summary>
        /// Create a new message factory context.
        /// </summary>
        /// <param name="ep">End point to parse messgaes from</param>
        /// <returns>A new context.</returns>
        /// <remarks>
        /// A context is used to parse messages from a specific endpoint.
        /// </remarks>
        internal MessageFactoryContext CreateNewContext(EndPoint ep)
        {
            MessageFactoryContext context = _builders.Dequeue();
            context.EndPoint = ep;
            return context;
        }

        internal Request CreateRequest(string method, string path, string version)
        {
            return new Request(method, path, version);
        }

        internal Response CreateResponse(string version, StatusCode statusCode, string reason)
        {
            return new Response(version, statusCode, reason);
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            RequestReceived(this, e);
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            ResponseReceived(this, e);
        }

        /// <summary>
        /// Release a used factoryContext.
        /// </summary>
        /// <param name="factoryContext"></param>
        internal void Release(MessageFactoryContext factoryContext)
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