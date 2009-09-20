using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SipSharp.Transactions
{
	public interface IClientTransaction : ITransaction
	{
        /// <summary>
        /// Process a response to the request.
        /// </summary>
        /// <param name="response">Received response</param>
        /// <param name="endPoint">End point that the response was received from.</param>
        /// <returns><c>true</c> if response was handled; otherwise <c>false</c>.</returns>
        bool Process(IResponse response, EndPoint endPoint);


        /// <summary>
        /// We like to reuse transaction objects. Remove all references
        /// to the transaction and reset all parameters.
        /// </summary>
	    void Cleanup();
	}
}
