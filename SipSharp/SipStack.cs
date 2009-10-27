using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;
using SipSharp.Transports;

namespace SipSharp
{
    public class SipStack : ISipStack
    {
        private MessageFactory _messageFactory;
        private TransportLayer _transportLayer;
        private TransactionManager _transactionManager;
        private DialogManager _dialogManager;
        private IPEndPoint _endPoint;
        private string _domain;
        private ILogger _logger = Logging.LogFactory.CreateLogger(typeof (SipStack));

        private SubscriberList<EventHandler<StackRequestEventArgs>> _requestSubscribers =
            new SubscriberList<EventHandler<StackRequestEventArgs>>();

        public SipStack()
        {
            HeaderFactory hf = new HeaderFactory();
            hf.AddDefaultParsers();
            _messageFactory = new MessageFactory(hf);
            _transportLayer = new TransportLayer(_messageFactory);
            _transportLayer.RequestReceived += OnRequest;
            _transportLayer.ResponseReceived += OnResponse;
            _transactionManager = new TransactionManager(_transportLayer);

        }

        public void Start(IPEndPoint ep)
        {
            UdpTransport transport = new UdpTransport(_messageFactory) {Port = 5060};
            transport.Start(ep);
            _transportLayer.Register(transport);
            _transportLayer.Start();
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
/*
            If there are any client transactions in existence, the client
            transport uses the matching procedures of Section 17.1.3 to attempt
            to match the response to an existing transaction.  If there is a
            match, the response MUST be passed to that transaction.  Otherwise,
            the response MUST be passed to the core (whether it be stateless
            proxy, stateful proxy, or UA) for further processing.  Handling of
            these "stray" responses is dependent on the core (a proxy will
            forward them, while a UA will discard, for example). 
*/
            if (_transactionManager.Process(e.Response))
            {
                _logger.Trace("Response " + e.Response + " handled by transaction.");
            }
            else
            {
                
            }
        }

        /// <summary>
        /// A request was received from the transaction layer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRequest(object sender, RequestEventArgs e)
        {
            _logger.Trace("Received " + e.Request + " from "  + e.RemoteEndPoint);

            if (_transactionManager.Process(e.Request))
                return;

            StackRequestEventArgs args = new StackRequestEventArgs(e.Request);
            _requestSubscribers.Invoke(e.Request.Method, handler => handler(this, args));
        }

        public void Send(IRequest request)
        {
            _transactionManager.CreateClientTransaction(request);
        }

        public void Send(IResponse response)
        {
            if (!_transactionManager.Process(response))
            {
                _logger.Error("Failed to find request for: " + response);
                throw new InvalidOperationException("Cannot send responses outside transactions.");
            }
        }

        public IServerTransaction CreateServerTransaction(IRequest request)
        {
            return _transactionManager.CreateServerTransaction(request);
        }

        IDialog CreateDialog()
        {
            return null;
        }
        public event EventHandler DialogCreated;
        public event EventHandler DialogTerminated;
        public void RegisterMethod(string methodName, EventHandler<StackRequestEventArgs> handler)
        {
            _requestSubscribers.Register(methodName, handler);
        }
    }
}
