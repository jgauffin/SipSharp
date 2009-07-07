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
		/// Checks whether the response belongs to our request
		/// </summary>
		/// <param name="response">Response to check</param>
		/// <returns></returns>
		bool IsOurResponse(IResponse response);

        string ProcessResponse(IResponse response, EndPoint endPoint);
	}
}
