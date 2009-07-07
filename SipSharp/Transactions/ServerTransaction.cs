using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    /// <summary>
    /// The server transaction is responsible for the delivery of requests to the TU and the reliable transmission of responses.
    /// </summary>
    class ServerTransaction
    {
        private TransactionState _state;
        private TransportManager _transport;
        private IRequest _request;
    	private EndPoint _endPoint;
    	private ILogWriter _logger = LogFactory.CreateLogger(typeof(ServerTransaction));
    	private IResponse _provisional;
    	private IResponse _response;

		/// <summary>
		/// Retransmission timer
		/// </summary>
    	private Timer _timerG;
		int _timerGValue;

		/// <summary>
		/// Timer H determines when the server
		/// transaction abandons retransmitting the response.  Its value is
		/// chosen to equal Timer B, the amount of time a client transaction will
		/// continue to retry sending a request
		/// </summary>
    	private Timer _timerH;

		/// <summary>
		/// <para>
		/// When this state is entered, timer I is set to fire in T4
		/// seconds for unreliable transports, and zero seconds for reliable
		/// transports.  Once timer I fires, the server MUST transition to the
		/// "Terminated" state.
		/// <para>
		/// <para>
		/// The server transaction remains in this state until Timer J fires, at
		/// which point it MUST transition to the "Terminated" state.		
		/// </para>
		/// </summary>
    	private Timer _timerIJ;

		public void HandleRequest(IRequest request)
		{
			if (_request != null)
			{
				// If a retransmission of the
				// request is received while in the "Proceeding" state, the most
				// recently sent provisional response MUST be passed to the transport
				// layer for retransmission.			
				_request = request;
				if (_state == TransactionState.Proceeding)
					Send(_provisional);
				// While in the "Completed" state, the
				// server transaction MUST pass the final response to the transport
				// layer for retransmission whenever a retransmission of the request is
				// received.  
				else
					Send(_response);
				return;
			}

			if (request.Method == "INVITE")
				SetupInvite(request);
			else
			{
				SetupNonInvite(request);
			}
		}

        protected virtual void SetupInvite(IRequest request)
        {
            // When a server transaction is constructed for a request, it enters the
            // "Proceeding" state.  The server transaction MUST generate a 100
            // (Trying) response unless it knows that the TU will generate a
            // provisional or final response within 200 ms, in which case it MAY
            // generate a 100 (Trying) response.  
			_state = TransactionState.Proceeding;
			IResponse response = request.CreateResponse(StatusCode.Trying, "Trying.");
			Send(response);

            // This provisional response is
            // needed to quench request retransmissions rapidly in order to avoid
            // network congestion.  The 100 (Trying) response is constructed
            // according to the procedures in Section 8.2.6, except that the
            // insertion of tags in the To header field of the response (when none
            // was present in the request) is downgraded from MAY to SHOULD NOT.
            // The request MUST be passed to the TU.
            RequestReceived(this, new RequestEventArgs(_request, _endPoint));
        }

		protected virtual void SetupNonInvite(IRequest request)
		{
			// The state machine is initialized in the "Trying" state and is passed
			// a request other than INVITE or ACK when initialized.  This request is
			// passed up to the TU.  Once in the "Trying" state, any further request
			// retransmissions are discarded.  A request is a retransmission if it
			// matches the same server transaction, using the rules specified in
			// Section 17.2.3.
			_state = TransactionState.Trying;

			if (request.Method == "ACK")
				throw new InvalidOperationException("Expected any other type than ACK and INVITE");
		}


		protected void Send(IResponse response)
		{
			if (_request.Method == "INVITE")
				SendInviteResponse(response);
			else
				SendNonInviteResponse(response);
		}

		private void SendNonInviteResponse(IResponse response)
		{
			// Any other final responses passed by the TU to the server
			// transaction MUST be discarded while in the "Completed" state.
			if (_state == TransactionState.Completed)
				return;

			// While in the "Trying" state, if the TU passes a provisional response
			// to the server transaction, the server transaction MUST enter the
			// "Proceeding" state.  
			if (_state == TransactionState.Trying && StatusCodeHelper.Is1xx(response))
				_state = TransactionState.Proceeding;

			// The response MUST be passed to the transport
			// layer for transmission.  Any further provisional responses that are
			// received from the TU while in the "Proceeding" state MUST be passed
			// to the transport layer for transmission.  
			
			// If the TU passes a final response (status
			// codes 200-699) to the server while in the "Proceeding" state, the
			// transaction MUST enter the "Completed" state, and the response MUST
			// be passed to the transport layer for transmission.
			if (_state == TransactionState.Proceeding)
			{
				Send(response);

				if (!StatusCodeHelper.Is1xx(response))
				{
					_state = TransactionState.Completed;
					// When the server transaction enters the "Completed" state, it MUST set
					// Timer J to fire in 64*T1 seconds for unreliable transports, and zero
					// seconds for reliable transports.  
					if (_timerIJ == null)
						_timerIJ = new Timer(OnTerminate);
					_timerIJ.Change(TransactionManager.T1*64, Timeout.Infinite);
				}
			}

			_response = response;
		}

    	private void SendInviteResponse(IResponse response)
    	{
    		if (StatusCodeHelper.Is1xx(response) && _state != TransactionState.Trying)
    		{
    			_logger.Write(this, LogLevel.Info, "Ignoring provisional response since we are not in Trying state.");
    			return;
    		}

    		if (StatusCodeHelper.Is1xx(response))
    			_provisional = response;


    		_response = response;
    		_transport.Send(_endPoint, response);

    		if (StatusCodeHelper.Is2xx(response))
    		{
    			_state = TransactionState.Terminated;
    			Terminated.BeginInvoke(this, EventArgs.Empty, null, null);
    		}
    		else if (StatusCodeHelper.Is3456xx(response))
    		{
    			_state = TransactionState.Completed;
    			if (_request.Via.First.Protocol == "UDP")
    			{
    				_timerGValue = TransactionManager.T1;
    				if (_timerG == null)
    					_timerG = new Timer(OnRetransmission);
    				_timerG.Change(_timerGValue, Timeout.Infinite);
    			}
    			if (_timerH == null)
    				_timerH = new Timer(OnTimeout);
    			_timerH.Change(TransactionManager.T1 * 64, Timeout.Infinite);
    		}
    	}

    	public void SetConfirmed()
		{
			if (_timerIJ == null) 
				_timerIJ = new Timer(OnTerminate);
			_timerIJ.Change(TransactionManager.T4, Timeout.Infinite);
			_state = TransactionState.Confirmed;
		}

		/// <summary>
		/// Terminate transaction
		/// </summary>
		protected virtual void Terminate()
		{
			_state = TransactionState.Terminated;
			Terminated(this, EventArgs.Empty);
		}

    	private void OnTerminate(object state)
    	{
			Terminate();
		}

    	private void OnTimeout(object state)
    	{
			// If timer H fires while in the "Completed" state, it implies that the
			// ACK was never received.  In this case, the server transaction MUST
			// transition to the "Terminated" state, and MUST indicate to the TU
			// that a transaction failure has occurred.
    		TimeoutTriggered(this, EventArgs.Empty);
			Terminate();
		}

    	private void OnRetransmission(object state)
    	{
			// If an ACK is received while the server transaction is in the
			// "Completed" state, the server transaction MUST transition to the
			// "Confirmed" state.  As Timer G is ignored in this state, any
			// retransmissions of the response will cease.
			if (_state == TransactionState.Confirmed)
				return;

			// If timer G fires, the response
			// is passed to the transport layer once more for retransmission, and
			// timer G is set to fire in MIN(2*T1, T2) seconds.  From then on, when
			// timer G fires, the response is passed to the transport again for
			// transmission, and timer G is reset with a value that doubles, unless
			// that value exceeds T2, in which case it is reset with the value of
			// T2.
			_transport.Send(_endPoint, _response);
    		_timerGValue = Math.Min(_timerGValue*2, TransactionManager.T2);
    		_timerG.Change(_timerGValue, Timeout.Infinite);
    	}

    	/// <summary>
        /// A request have been received from the transport layer.
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestReceived = delegate { };

		/// <summary>
		/// A timeout have occurred.
		/// </summary>
		public event EventHandler TimeoutTriggered = delegate { };

		/// <summary>
		/// Transaction have been terminated.
		/// </summary>
    	public event EventHandler Terminated = delegate { };
    }
}
