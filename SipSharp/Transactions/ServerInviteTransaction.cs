using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    class ServerInviteTransaction
    {
        private TransactionState _state;
        private TransportManager _transport;
        private IRequest _request;

        public ServerInviteTransaction(TransportManager transportManager, IRequest request)
        {
            _transport = transportManager;
            _state = TransactionState.Proceeding;
            _request = request;

            if (request.Method == "ACK")
                throw new InvalidOperationException("Expected any other type than ACK and INVITE");

            // The server transaction MUST generate a 100
            // (Trying) response unless it knows that the TU will generate a
            // provisional or final response within 200 ms, in which case it MAY
            // generate a 100 (Trying) response
            Send(request.CreateResponse(StatusCode.Trying, "We are trying here..."));
        }


        public void Send(IResponse response)
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
                _transport.Send(response);

                if (!StatusCodeHelper.Is1xx(response))
                {
                    _state = TransactionState.Completed;
                    // When the server transaction enters the "Completed" state, it MUST set
                    // Timer J to fire in 64*T1 seconds for unreliable transports, and zero
                    // seconds for reliable transports.  
                    if (_timerIJ == null)
                        _timerIJ = new Timer(OnTerminate);
                    _timerIJ.Change(TransactionManager.T1 * 64, Timeout.Infinite);
                }
            }

            _response = response;
        }


    }
}
