using System;
using System.Net;
using System.Threading;
using SipSharp.Dialogs;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    /// <summary>
    /// Client transaction for all messages but Invite.
    /// </summary>
    public class ClientTransaction
    {
        /// <summary>
        /// Retransmit timer for Non-Invite transactions.
        /// </summary>
        private readonly Timer _timerE;

        private readonly ITransportManager _transportManager;
        private IDialog _dialog;
        private EndPoint _endPoint;
        private IRequest _request;
        private TransactionState _state;
        private Timer _timer1;

        /// <summary>
        /// Controls request retransmissions
        /// </summary>
        private Timer _timerA;

        private int _timerAValue;


        private int _timerEValue = TransactionManager.T1*2;

        /// <summary>
        /// Timeout timer for Non-Invite transactions.
        /// </summary>
        private Timer _timerF;

        /// <summary>
        /// Time in completed state timer
        /// </summary>
        /// <remarks>
        /// The "Completed" state exists to buffer any additional response retransmissions 
        /// that may be received.
        /// </remarks>
        private Timer _timerK;


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTransaction"/> class.
        /// </summary>
        /// <param name="transportManager">Used to transport messages.</param>
        /// <param name="message">Request to process.</param>
        public ClientTransaction(ITransportManager transportManager, IMessage message)
        {
            _transportManager = transportManager;

            if (!(message is IRequest))
            {
                throw new SipSharpException("Client transaction can only be created with requests");
            }
            IRequest request = (Request) message;
            if (request.Method == "INVITE")
                throw new SipSharpException("Use ClientInviteTransaction for invite requests.");

            _state = TransactionState.Trying;
            _timerF = new Timer(OnTimeout, null, TransactionManager.T1*64, Timeout.Infinite);
            if (request.Via.First.Protocol == "UDP")
            {
                _timerEValue = TransactionManager.T1;
                _timerE = new Timer(OnRetransmission, null, _timerEValue, Timeout.Infinite);
            }
            _transportManager.Send(_endPoint, request);
        }

        public void AddMessage(IMessage message)
        {
            var response = (IResponse) message;
            ProcessResponse(response);
        }

        /// <summary>
        /// Determines if this transaction equals a sip message.
        /// </summary>
        /// <param name="obj">Another transaction object, or a <see cref="IResponse"/>.</param>
        /// <returns>True if equal; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IResponse)
            {
                //A response matches a client transaction under two conditions:

                //  1.  If the response has the same value of the branch parameter in
                //      the top Via header field as the branch parameter in the top
                //      Via header field of the request that created the transaction.

                //  2.  If the method parameter in the CSeq header field matches the
                //      method of the request that created the transaction.  The
                //      method is needed since a CANCEL request constitutes a
                //      different transaction, but shares the same value of the branch
                //      parameter.
                var response = (IResponse) obj;
                return response.Via.First.Branch == _request.Via.First.Branch
                       && response.CSeq.Method == _request.CSeq.Method;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void OnRetransmission(object state)
        {
            /* If Timer E fires while in the "Proceeding" state, the request MUST be
                   passed to the transport layer for retransmission, and Timer E MUST be
                   reset with a value of T2 seconds.  
                 */
            _timerEValue = Math.Min(_timerEValue*2, TransactionManager.T2);
            _timerE.Change(_timerEValue, Timeout.Infinite);
            _transportManager.Send(_endPoint, _request);
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
            _state = TransactionState.Terminated;

            /* If Timer F fires while the client transaction is still in the
                 * "Trying" state, the client transaction SHOULD inform the TU about the
                 * timeout, and then it SHOULD enter the "Terminated" state                
                 */
            if (_state == TransactionState.Trying)
                TimeoutTriggered(this, EventArgs.Empty);
        }

        private void ProcessResponse(IResponse response)
        {
            if (_state == TransactionState.Terminated)
                return;

            /* If a provisional response is received while in the "Trying" state, the
             * response MUST be passed to the TU, and then the client transaction
             * SHOULD move to the "Proceeding" state.  
             */
            if (StatusCodeHelper.Is1xx(response))
            {
                _state = TransactionState.Proceeding;
            }

                /* If a final response (status codes 200-699) is received while in the "Trying" state,
             * the response MUST be passed to the TU, and the client transaction MUST transition
             * to the "Completed" state
             * 
             * same stuff is stated for proceeding state.
             */
            else
            {
                _state = TransactionState.Completed;

                // Once the client transaction enters the "Completed" state, it MUST set
                // Timer K to fire in T4 seconds for unreliable transports, and zero
                // seconds for reliable transports
                if (_timerK == null)
                    _timerK = new Timer(OnTerminate, null, Timeout.Infinite, Timeout.Infinite);
                _timerK.Change(TransactionManager.T4, Timeout.Infinite);
            }

            ResponseReceived(this, new ResponseEventArgs(response, _endPoint));
        }


        private void Setup(IRequest request)
        {
        }

        public void TriggerTransportFailed()
        {
            _state = TransactionState.Terminated;
            TransportFailed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invoked if transportation failed.
        /// </summary>
        public event EventHandler TransportFailed = delegate { };

        /// <summary>
        /// A timeout have occurred.
        /// </summary>
        public event EventHandler TimeoutTriggered = delegate { };

        /// <summary>
        /// A response have been received.
        /// </summary>
        public event EventHandler<ResponseEventArgs> ResponseReceived = delegate { };

        /// <summary>
        /// Transaction have been terminated.
        /// </summary>
        /// <remarks>
        /// Event used to dispose transaction or to re-use it.
        /// </remarks>
        public event EventHandler Terminated = delegate { };
    }
}