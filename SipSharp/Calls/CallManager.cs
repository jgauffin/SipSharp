﻿using System;
using System.Collections.Generic;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Servers.Switch;
using SipSharp.Transactions;

namespace SipSharp.Calls
{
    public class CallManager
    {
        private readonly ISipStack _stack;
    	private readonly DialogManager _dialogManager;
    	Dictionary<string, Call> _calls = new Dictionary<string, Call>();
        private ILogger _logger = LogFactory.CreateLogger(typeof (CallManager));

        public CallManager(ISipStack stack, DialogManager dialogManager)
        {
        	_stack = stack;
        	_dialogManager = dialogManager;
        }


    	/// <summary>
		/// Unsubscribe all events so that the transaction
		/// can be garbage collected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTerminate(object sender, EventArgs e)
		{
			IClientTransaction transaction = (IClientTransaction)sender;
			transaction.ResponseReceived -= OnResponse;
			transaction.TimedOut -= OnTimeout;
			transaction.TransportFailed -= OnTransportFailed;
			transaction.Terminated -= OnTerminate;
		}




		private void OnTransportFailed(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void OnTimeout(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}


		private void OnResponse(object sender, ResponseEventArgs e)
		{
			// Answer to an invite that we sent.
			if (e.Transaction.Request.Method == SipMethod.INVITE)
			{
				_dialogManager.CreateClientDialog(e.Transaction.Request, e.Response);
				return;
			}
		}

		public Call Call(Contact from, Contact to)
		{
			IRequest request = _stack.CreateRequest(SipMethod.INVITE, from, to);
			IClientTransaction transaction = _stack.Send(request);
			transaction.ResponseReceived += OnResponse;
			transaction.TimedOut += OnTimeout;
			transaction.TransportFailed += OnTransportFailed;
			transaction.Terminated += OnTerminate;

			Call call = new Call
			{
				Destination = new CallParty { Contact = to },
				Caller = new CallParty { Contact = from },
				Id = request.CallId,
				Reason = CallReasons.Direct,
				State = CallState.Proceeding
			};
			_calls.Add(call.Id, call);
			CallChanged(this, new CallEventArgs(call));
			return call;
		}


        public event EventHandler<CallEventArgs> CallChanged = delegate{};
    }
}