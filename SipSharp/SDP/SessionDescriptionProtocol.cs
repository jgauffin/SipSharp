using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SipSharp.SDP
{
	/// <summary>
	/// SDP is intended for describing multimedia sessions for the purposes of session
	/// announcement, session invitation, and other forms of multimedia session
	/// initiation.
	/// </summary>
	public class SessionDescriptionProtocol
	{
		public void Parse(Stream stream)
		{
			StreamReader streamReader = new StreamReader(stream);
			StringReader reader = new StringReader(streamReader.ReadToEnd());

		}
	}
}
