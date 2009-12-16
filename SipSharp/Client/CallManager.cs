using System;
using System.Collections.Generic;
using SipSharp.Calls;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Servers.Switch;
using SipSharp.Transactions;

namespace SipSharp.Client
{
    public class CallManager
    {
        private readonly ISipStack _stack;
        Dictionary<string, Call> _calls = new Dictionary<string, Call>();
        private ILogger _logger = LogFactory.CreateLogger(typeof (CallManager));

        public CallManager(ISipStack stack)
        {
            _stack = stack;
        }

        void MakeCall(Contact from, Contact to)
        {
            IRequest request = _stack.CreateRequest(SipMethod.INVITE, from, to);
            IClientTransaction transaction = _stack.Send(request);
            transaction.ResponseReceived += OnResponse;
            transaction.TimedOut += OnTimeout;
            transaction.TransportFailed += OnTransportFailed;

            Call call = new Call
                            {
                                Destination = to,
                                Caller = from,
                                Id = request.CallId,
                                Reason = CallReason.Direct,
                                State = CallState.Proceeding
                            };
            _calls.Add(call.Id, call);
            CallChanged(this, new CallEventArgs(call));
        }


        private string CreateCallId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
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
            if (e.Transaction.Request.Method == SipMethod.INVITE)
            {
                if (!e.Transaction.Request.To.HasParameter("tag") && StatusCodeHelper.Is1xx(e.Response))
                {
                    _logger.Debug("Request do not have a to tag, ignoring response " + e.Response + " for request " + e.Transaction.Request);
                    return;
                }

                // See RFC3261 Section 12.1 Creation of a Dialog
                if (StatusCodeHelper.Is3456xx(e.Response))
                {
                    _logger.Warning("Error response: " + e.Response + " to request " + e.Transaction.Request);
                    return;
                }

                // RFC 3261 Section 12.1.2
                Dialog dialog = new Dialog();

                // If the request was sent over TLS, and the Request-URI contained a SIPS URI, the "secure" flag is set to TRUE.
                dialog.IsSecure = e.Transaction.Request.Uri.Scheme = "sips";

                //The route set MUST be set to the list of URIs in the Record-Route
                //header field from the response, taken in reverse order and preserving
                //all URI parameters.  If no Record-Route header field is present in
                //the response, the route set MUST be set to the empty set.  This route
                //set, even if empty, overrides any pre-existing route set for future
                //requests in this dialog.  
                if (e.Response.Headers.Contains("Record-Route"))
                {
                    Route recordRoute = new Route("Route");
                    Route route = (Route) e.Response.Headers["Record-Route"];
                    foreach (var entry in route)
                        recordRoute.Items.Insert(0, (RouteEntry) entry.Clone());
                    dialog.Route = route;
                }

                // The remote target MUST be set to the URI
                //from the Contact header field of the response.
                dialog.RemoteTarget = e.Response.Contact;

                // The local sequence number MUST be set to the value of the sequence
                // number in the CSeq header field of the request.  The remote sequence
                // number MUST be empty (it is established when the remote UA sends a
                // request within the dialog).  The call identifier component of the
                // dialog ID MUST be set to the value of the Call-ID in the request.
                //
                // The local tag component of the dialog ID MUST be set to the tag in
                // the From field in the request, and the remote tag component of the
                // dialog ID MUST be set to the tag in the To field of the response.  A
                // UAC MUST be prepared to receive a response without a tag in the To
                // field, in which case the tag is considered to have a value of null.
                dialog.LocalSequenceNumber = e.Transaction.Request.CSeq.Number;
                dialog.RemoteSequenceNumber = 0;
                dialog.CallId = e.Transaction.Request.CallId;
                dialog.LocalTag = e.Transaction.Request.From.Parameters["tag"];
                dialog.RemoteTag = e.Response.To.Parameters["tag"];

                // The remote URI MUST be set to the URI in the To field, and the local
                // URI MUST be set to the URI in the From field.
                dialog.RemoteUri = e.Transaction.Request.To.Uri;
                dialog.LocalUri = e.Transaction.Request.From.Uri;
            }
        }

        public event EventHandler<CallEventArgs> CallChanged = delegate{};
    }
}
