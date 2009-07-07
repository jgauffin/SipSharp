using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Transports;

namespace SipSharp.Transactions
{
    class TransactionManager
    {
        private ITransportManager _transports;
		

        /// <summary>
        /// Estimated round-trip time (RTT)
        /// </summary>
        public const int T1 = 500;

        /// <summary>
        /// Max retransmit time.
        /// </summary>
        public const int T2 = 4000;

        /// <summary>
        /// The amount of time the network will take to clear messages between client and
        /// server transactions
        /// </summary>
        public const int T4 = 5000;

        public TransactionManager(ITransportManager transports)
        {
            _transports = transports;
            _transports.RequestReceived += OnRequest;
            _transports.ResponseReceived += OnResponse;
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
            // A response matches a client transaction under two conditions:

            //  1.  If the response has the same value of the branch parameter in
            //      the top Via header field as the branch parameter in the top
            //      Via header field of the request that created the transaction.

            //  2.  If the method parameter in the CSeq header field matches the
            //      method of the request that created the transaction.  The
            //      method is needed since a CANCEL request constitutes a
            //      different transaction, but shares the same value of the branch
            //      parameter.        


          }

		private void OnRequest(object sender, RequestEventArgs e)
		{
			// RFC 3261, Section 17.2.1: If a request
			// retransmission is received while in the "Proceeding" state, the most
			// recent provisional response that was received from the TU MUST be
			// passed to the transport layer for retransmission.			

		}

        public ClientTransaction CreateClientTransaction(IMessage message)
        {
            ClientTransaction transaction = new ClientTransaction(_transports, null);
            return transaction;
        }


    }
}
