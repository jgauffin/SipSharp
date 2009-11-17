using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages;
using SipSharp.Messages.Headers;

namespace SipSharp.Servers.Agent
{
    class UserAgentClient
    {
        private string _registerCallId;
        private int _sequenceNumber;
        private Route _defaultRoute = new Route(Route.ROUTE_NAME);
        private Contact _contact;

        public UserAgentClient()
        {
            _registerCallId = CreateRandomString();
        }



        /// <summary>
        /// Create a request
        /// </summary>
        /// <remarks>
        /// <para>
        ///   A valid SIP request formulated by a UAC MUST, at a minimum, contain
        ///   the following header fields: To, From, CSeq, Call-ID, Max-Forwards,
        ///   and Via; all of these header fields are mandatory in all SIP
        ///   requests.  These six header fields are the fundamental building
        ///   blocks of a SIP message, as they jointly provide for most of the
        ///   critical message routing services including the addressing of
        ///   messages, the routing of responses, limiting message propagation,
        ///   ordering of messages, and the unique identification of transactions.
        ///   These header fields are in addition to the mandatory request line,
        ///   which contains the method, Request-URI, and SIP version.
        /// <para></para>
        ///   Examples of requests sent outside of a dialog include an INVITE to
        ///   establish a session (RFC3261 - Section 13) and an OPTIONS to query for
        ///   capabilities (RFC3261 - Section 11).            
        /// </para>
        /// <para>
        /// Read section 8.1.1 in RFC3261 for more information
        /// </para>
        /// </remarks>
        private IRequest CreateRequest(string method, Contact from, Contact to)
        {
            Request request = new Request(method, to.Uri, "SIP/2.0");
            request.To = to;
            request.From = from;
            request.From.Parameters["tag"] = CreateRandomString(8);
            request.CallId = method == SipMethod.REGISTER ? _registerCallId : CreateRandomString();
            request.CSeq = new CSeq(GetNextSequenceNumber(), method);
            request.MaxForwards = 70;
            request.Contact = _contact;
            if (_defaultRoute != null && _defaultRoute.Items.Count > 0)
                request.Headers.Add(Route.ROUTE_NAME, (IHeader)_defaultRoute.Clone());

            return request;
        }

        protected virtual string CreateRandomString(int length)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(length);
        }

        protected virtual string CreateRandomString()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        protected int GetNextSequenceNumber()
        {
            if (_sequenceNumber == int.MaxValue)
                _sequenceNumber = 1;
            else
                ++_sequenceNumber;
            return _sequenceNumber;
        }
    }
}
