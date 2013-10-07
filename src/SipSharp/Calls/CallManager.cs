using System;
using System.Collections.Generic;
using SipSharp.Dialogs;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Transactions;

namespace SipSharp.Calls
{
    public class CallManager
    {
        private readonly Dictionary<string, Call> _calls = new Dictionary<string, Call>();
        private readonly DialogManager _dialogManager;
        private readonly ISipStack _stack;
        private ILogger _logger = LogFactory.CreateLogger(typeof (CallManager));

        public CallManager(ISipStack stack, DialogManager dialogManager)
        {
            _stack = stack;
            _dialogManager = dialogManager;
        }

        public Call Call(Contact from, Contact to)
        {
            IRequest request = _stack.CreateRequest(SipMethod.INVITE, from, to);
            IClientTransaction transaction = _stack.Send(request);
            transaction.ResponseReceived += OnResponse;
            transaction.TimedOut += OnTimeout;
            transaction.TransportFailed += OnTransportFailed;
            transaction.Terminated += OnTerminate;

            var call = new Call
                           {
                               Destination = new CallParty {Contact = to},
                               Caller = new CallParty {Contact = from},
                               Id = request.CallId,
                               Reason = CallReasons.Direct,
                               State = CallState.Proceeding
                           };
            _calls.Add(call.Id, call);
            CallChanged(this, new CallEventArgs(call));
            return call;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            // Answer to an invite that we sent.
            if (e.Transaction.Request.Method == SipMethod.INVITE)
            {
                Dialog dialog = _dialogManager.CreateClientDialog(e.Transaction.Request, e.Response);

                return;
            }
        }


        /// <summary>
        /// Unsubscribe all events so that the transaction
        /// can be garbage collected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTerminate(object sender, EventArgs e)
        {
            var transaction = (IClientTransaction) sender;
            transaction.ResponseReceived -= OnResponse;
            transaction.TimedOut -= OnTimeout;
            transaction.TransportFailed -= OnTransportFailed;
            transaction.Terminated -= OnTerminate;
        }


        private void OnTimeout(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnTransportFailed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// A call (or it's state) have changed.
        /// </summary>
        public event EventHandler<CallEventArgs> CallChanged = delegate { };
    }

    /// <summary>
    /// Event arguments for various call events.
    /// </summary>
    public class CallEventArgs : EventArgs
    {
        /// <summary>
        /// Gets call.
        /// </summary>
        public Call Call { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallEventArgs"/> class.
        /// </summary>
        /// <param name="call">The call.</param>
        public CallEventArgs(Call call)
        {
            Call = call;
        }
    }
}