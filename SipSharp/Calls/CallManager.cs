using System;
using System.Collections.Generic;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Servers.Switch;
using SipSharp.Transactions;

namespace SipSharp.Calls
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

        public bool Process(IRequest request)
        {
            return true;
        }

        public Call Call(Contact from, Contact to)
        {
            IRequest request = _stack.CreateRequest(SipMethod.INVITE, from, to);
            IClientTransaction transaction = _stack.Send(request);
            transaction.ResponseReceived += OnResponse;
            transaction.TimedOut += OnTimeout;
            transaction.TransportFailed += OnTransportFailed;
            transaction.Terminated += OnTerminate;

            Call call = new Call
                            {
                                Destination = new CallParty {Contact = to},
                                Caller = new CallParty{Contact = from},
                                Id = request.CallId,
                                Reason = CallReasons.Direct,
                                State = CallState.Proceeding
                            };
            _calls.Add(call.Id, call);
            CallChanged(this, new CallEventArgs(call));
            return call;
        }

        /// <summary>
        /// Unsubscribe all events so that the transaction
        /// can be garbage collected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTerminate(object sender, EventArgs e)
        {
            IClientTransaction transaction = (IClientTransaction)sender;
            transaction.ResponseReceived -= OnResponse;
            transaction.TimedOut -= OnTimeout;
            transaction.TransportFailed -= OnTransportFailed;
            transaction.Terminated -= OnTerminate;
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
            // Answer to an invite that we sent.
            if (e.Transaction.Request.Method == SipMethod.INVITE)
            {
                Dialog dialog = CreateClientDialog(e.Transaction.Request, e.Response);
                return;
            }
        }

        /// <summary>
        /// Create a dialog when we get a response to our INVITE request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>
        /// Described in RFC321 Section 12.1.2 UAC Behavior
        /// </remarks>
        private Dialog CreateClientDialog(IRequest request, IResponse response)
        {
            // When a UAC sends a request that can establish a dialog (such as an
            // INVITE) it MUST provide a SIP or SIPS URI with global scope (i.e.,
            // the same SIP URI can be used in messages outside this dialog) in the
            // Contact header field of the request.  If the request has a Request-
            // URI or a topmost Route header field value with a SIPS URI, the
            // Contact header field MUST contain a SIPS URI.
            Route route = (Route) request.Headers[Route.ROUTE_LNAME];
            if (request.Uri.Scheme == "sips" || (route != null && route.First.Uri.Scheme == "sips"))
            {
                if (request.Contact.Uri.Scheme != "sips")
                    throw new BadRequestException("Contact must use 'sips' if Request URI or first VIA uses 'sips'");
            }

            // When a UAC receives a response that establishes a dialog, it
            // constructs the state of the dialog.  This state MUST be maintained
            // for the duration of the dialog.
            Dialog dialog = new Dialog();
            dialog.State = DialogState.Early;

            // If the request was sent over TLS, and the Request-URI contained a
            // SIPS URI, the "secure" flag is set to TRUE.
            dialog.IsSecure = request.Uri.Scheme.Equals("sips");

            // The route set MUST be set to the list of URIs in the Record-Route
            // header field from the response, taken in reverse order and preserving
            // all URI parameters.  If no Record-Route header field is present in
            // the response, the route set MUST be set to the empty set.  This route
            // set, even if empty, overrides any pre-existing route set for future
            // requests in this dialog.  The remote target MUST be set to the URI
            // from the Contact header field of the response.
            dialog.RouteSet = response.Headers.Contains(Route.RECORD_ROUTE_LNAME)
                                  ? CopyAndReverse((Route)response.Headers[Route.RECORD_ROUTE_LNAME])
                                  : new Route("Route");


            // The local sequence number MUST be set to the value of the sequence
            // number in the CSeq header field of the request.  The remote sequence
            // number MUST be empty (it is established when the remote UA sends a
            // request within the dialog).  
            dialog.LocalSequenceNumber = request.CSeq.Number;
            dialog.RemoteSequenceNumber = 0;
            
            // The call identifier component of the
            // dialog ID MUST be set to the value of the Call-ID in the request.
            dialog.CallId = request.CallId;

            // The local tag component of the dialog ID MUST be set to the tag in
            // the From field in the request, and the remote tag component of the
            // dialog ID MUST be set to the tag in the To field of the response.  A
            // UAC MUST be prepared to receive a response without a tag in the To
            // field, in which case the tag is considered to have a value of null.
            //   This is to maintain backwards compatibility with RFC 2543, which
            //   did not mandate To tags.
            dialog.LocalTag = request.From.Parameters["tag"];
            dialog.RemoteTag = response.To.Parameters["tag"];

            // The remote URI MUST be set to the URI in the To field, and the local
            // URI MUST be set to the URI in the From field.            
            dialog.RemoteUri = request.To.Uri;
            dialog.LocalUri = request.From.Uri;

            return dialog;
        }

        /// <summary>
        /// Create a dialog from as an user agent server.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>
        /// Described in RFC3261 Section 12.1.1 UAS behavior
        /// </remarks>
        private Dialog CreateServerDialog(IServerTransaction transaction, IResponse response)
        {
            if (!transaction.Request.To.HasParameter("tag") && StatusCodeHelper.Is1xx(response))
            {
                _logger.Debug("Request do not have a to tag, ignoring response " + response + " for request " + transaction.Request);
                return null;
            }

            // See RFC3261 Section 12.1 Creation of a Dialog
            if (StatusCodeHelper.Is3456xx(response))
            {
                _logger.Warning("Error response: " + response + " to request " + transaction.Request);
                return null;
            }

            // RFC 3261 Section 12.1.2
            Dialog dialog = new Dialog();

            // If the request was sent over TLS, and the Request-URI contained a SIPS URI, 
            // the "secure" flag is set to TRUE.
            dialog.IsSecure = transaction.Request.Uri.Scheme == "sips";

            //The route set MUST be set to the list of URIs in the Record-Route
            //header field from the request, taken in order and preserving all URI
            //parameters.  If no Record-Route header field is present in the
            //request, the route set MUST be set to the empty set.  This route set,
            //even if empty, overrides any pre-existing route set for future
            //requests in this dialog.  
            dialog.RouteSet = response.Headers.Contains(Route.RECORD_ROUTE_LNAME)
                                  ? CopyAndReverse((Route) response.Headers[Route.RECORD_ROUTE_LNAME])
                                  : new Route("Route");


            // The remote target MUST be set to the URI
            //from the Contact header field of the response.
            dialog.RemoteTarget = response.Contact;

            //   The remote sequence number MUST be set to the value of the sequence
            //   number in the CSeq header field of the request.  The local sequence
            //   number MUST be empty.  
            dialog.LocalSequenceNumber = transaction.Request.CSeq.Number;
            dialog.RemoteSequenceNumber = 0;
            
            
            // The call identifier component of the dialog ID
            //   MUST be set to the value of the Call-ID in the request.  The local
            //   tag component of the dialog ID MUST be set to the tag in the To field
            //   in the response to the request (which always includes a tag), and the
            //   remote tag component of the dialog ID MUST be set to the tag from the
            //   From field in the request.  A UAS MUST be prepared to receive a
            //   request without a tag in the From field, in which case the tag is
            //   considered to have a value of null.
            dialog.CallId = transaction.Request.CallId;
            dialog.LocalTag = transaction.Request.To.Parameters["tag"];
            dialog.RemoteTag = response.From.Parameters["tag"];

            // The remote URI MUST be set to the URI in the From field, and the
            // local URI MUST be set to the URI in the To field.
            dialog.RemoteUri = transaction.Request.From.Uri;
            dialog.LocalUri = transaction.Request.To.Uri;

            return dialog;
        }

        private Route CopyAndReverse(Route route)
        {
            Route recordRoute = new Route("Route");
            foreach (var entry in route)
                recordRoute.Items.Insert(0, (RouteEntry) entry.Clone());
            return recordRoute;
        }

        public event EventHandler<CallEventArgs> CallChanged = delegate{};
    }
}