﻿using System;
using System.Threading;
using SipSharp.Headers;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
	public class ClientInviteTransaction
	{
		private readonly IRequest _request;

		/// <summary>
		/// Controls request retransmissions
		/// </summary>
		private readonly Timer _timerA;

		/// <summary>
		/// Timer D reflects the amount of time that the server transaction can
		/// remain in the "Completed" state when unreliable transports are used.
		/// </summary>
		/// <remarks>
		/// <para>Should be at least 32 seconds.</para>
		/// <para>This is equal to Timer H in the INVITE server transaction.</para>
		/// </remarks>
		private readonly Timer _timerD;

		private readonly int _timerDValue = 32000;

		private readonly ITransportManager _transportManager;
		private TransactionState _state;
		private int _timerAValue;

		/// <summary>
		/// Controls transaction timeouts
		/// </summary>
		private Timer _timerB;

		public ClientInviteTransaction(ITransportManager transportManager, IMessage message)
		{
			_transportManager = transportManager;
			if (!(message is IRequest))
			{
				throw new SipSharpException("Client transaction can only be created with requests");
			}
			IRequest request = (Request) message;
			if (request.Method != "INVITE")
				throw new SipSharpException("Can only be used with invite transactions.");

			_state = TransactionState.Calling;
			_transportManager.Send(_endPoint, request);
			_request = request;

			// If an unreliable transport is being used, the client transaction MUST 
			// start timer A with a value of T1. If a reliable transport is being used, 
			// the client transaction SHOULD NOT start timer A 
			if (request.Via.First.Protocol == "UDP")
			{
				_timerAValue = TransactionManager.T1;
				_timerA = new Timer(OnRetransmission, null, _timerAValue, Timeout.Infinite);
			}
			else _timerDValue = 0;

			// For any transport, the client transaction MUST start timer B with a value of 64*T1 seconds 
			_timerB = new Timer(OnTimeout, null, TransactionManager.T1*64, Timeout.Infinite);

			_timerD = new Timer(OnTerminate, null, _timerDValue, Timeout.Infinite);
		}

		private void OnTerminate(object state)
		{
			/* If timer D fires while the client transaction is in the "Completed"
			 * state, the client transaction MUST move to the terminated state.
			 */
			if (_state == TransactionState.Completed)
			{
				_state = TransactionState.Terminated;
			}
		}

		private void OnTimeout(object state)
		{
			// If the client transaction is still in the "Calling" state when timer
			// B fires, the client transaction SHOULD inform the TU that a timeout has occurred.
			if (_state == TransactionState.Calling)
			{
				TimeoutTriggered(this, EventArgs.Empty);
				_state = TransactionState.Terminated;
			}
		}

		/// <summary>
		/// A timeout have occurred.
		/// </summary>
		public event EventHandler TimeoutTriggered = delegate { };

		private void OnRetransmission(object state)
		{
			/* RFC 3261 17.1.1.2.
	When timer A fires, the client transaction MUST retransmit the request by passing 
	it to the transport layer, and MUST reset the timer with a value of 2*T1.
                  
	These retransmissions SHOULD only be done while the client
	transaction is in the "calling" state.
			  
			  The formal definition of retransmit within the context of the transaction layer is to take the message
   previously sent to the transport layer and pass it to the transport
   layer once more.
*/
			if (_state != TransactionState.Calling)
				return;

			_timerAValue *= 2;
			_timerA.Change(_timerAValue, Timeout.Infinite);
			_transportManager.Send(_endPoint, _request);
		}


		public void ProcessResponse(IResponse response)
		{
			if (_state == TransactionState.Terminated)
				return; // Ignore everything in terminated until we get disposed.

			/*
						   If the client transaction receives a provisional response while in
						   the "Calling" state, it transitions to the "Proceeding" state. In the
						   "Proceeding" state, the client transaction SHOULD NOT retransmit the
						   request any longer. Furthermore, the provisional response MUST be
						   passed to the TU.  Any further provisional responses MUST be passed
						   up to the TU while in the "Proceeding" state.                    
						*/
			if (_state == TransactionState.Calling && StatusCodeHelper.Is1xx(response))
			{
				_timerA.Change(Timeout.Infinite, Timeout.Infinite);
				_state = TransactionState.Proceeding;
			}
			else if (_state == TransactionState.Calling || _state == TransactionState.Proceeding)
			{
				bool changeTimerD = false;
				/* When in either the "Calling" or "Proceeding" states, reception of a
				 * 2xx response MUST cause the client transaction to enter the
				 * "Terminated" state, and the response MUST be passed up to the TU.
				 */
				if (StatusCodeHelper.Is2xx(response))
				{
					_state = TransactionState.Terminated;
				}

					/*
				 * When in either the "Calling" or "Proceeding" states, reception of a
				 * response with status code from 300-699 MUST cause the client
				 * transaction to transition to "Completed".  
				 * 
				 * Report msg and send an ACK back.
				 */
				else if (StatusCodeHelper.Is3456xx(response))
				{
					_state = TransactionState.Completed;
					SendAck(response);
					changeTimerD = true;
				}

				ResponseReceived(this, new ResponseEventArgs(response, _endPoint));

				//triggering it here instead of in the if clause to make sure
				// that response have been processed successfully first.
				if (changeTimerD)
					_timerD.Change(_timerDValue, Timeout.Infinite);
			}
			else if (_state == TransactionState.Completed)
			{
				/*
				 * Any retransmissions of the final response that are received while in
				 * the "Completed" state MUST cause the ACK to be re-passed to the
				 * transport layer for retransmission, but the newly received response
				 * MUST NOT be passed up to the TU
				 */
				SendAck(response);
			}
		}


		private void SendAck(IResponse response)
		{
			_transportManager.Send(_endPoint, CreateAck(response));
		}

		protected virtual IRequest CreateAck(IResponse response)
		{
			/* The ACK request constructed by the client transaction MUST contain
			 * values for the Call-ID, From, and Request-URI that are equal to the
			 * values of those header fields in the request passed to the transport
			 * by the client transaction (call this the "original request").  
			 * 
			 * The To header field in the ACK MUST equal the To header field in the
			 * response being acknowledged, and therefore will usually differ from
			 * the To header field in the original request by the addition of the
			 * tag parameter.  
			 * 
			 * The ACK MUST contain a single Via header field, and
			 * this MUST be equal to the top Via header field of the original
			 * request.  
			 * 
			 * The CSeq header field in the ACK MUST contain the same
			 * value for the sequence number as was present in the original request,
			 * but the method parameter MUST be equal to "ACK".
			 * 
			 * If the INVITE request whose response is being acknowledged had Route
			 * header fields, those header fields MUST appear in the ACK.  This is
			 * to ensure that the ACK can be routed properly through any downstream
			 * stateless proxies.
			 */
			var ack = new Request("ACK", _request.Uri.ToString(), "SIP/2.0")
			          	{
			          		Method = "ACK",
			          		Uri = _request.Uri,
			          		CallId = _request.CallId,
			          		From = _request.From,
			          		To = response.To,
			          		CSeq = new CSeq(_request.CSeq.SeqNr, "ACK")
			          	};
			ack.Via.AddToTop(_request.Via.First);
			foreach (RouteEntry route in response.Route)
				ack.Headers.Add("Route", route.ToString());

			return ack;
		}

		/// <summary>
		/// A response have been received.
		/// </summary>
		public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };
	}
}