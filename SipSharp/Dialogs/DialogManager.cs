using System;
using System.Collections.Generic;
using SipSharp.Calls;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;

namespace SipSharp.Dialogs
{
    public class DialogManager
    {
        private readonly Dictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (CallManager));
        private readonly ISipStack _stack;
        private Dictionary<string, Call> _calls = new Dictionary<string, Call>();

        public DialogManager(ISipStack stack)
        {
            _stack = stack;
        }

        private Route CopyAndReverse(Route route)
        {
            var recordRoute = new Route("Route");
            foreach (var entry in route)
                recordRoute.Items.Insert(0, (RouteEntry) entry.Clone());
            return recordRoute;
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
        public Dialog CreateClientDialog(IRequest request, IResponse response)
        {
            // When a UAC sends a request that can establish a dialog (such as an
            // INVITE) it MUST provide a SIP or SIPS URI with global scope (i.e.,
            // the same SIP URI can be used in messages outside this dialog) in the
            // Contact header field of the request.  If the request has a Request-
            // URI or a topmost Route header field value with a SIPS URI, the
            // Contact header field MUST contain a SIPS URI.
            var route = (Route) request.Headers[Route.ROUTE_LNAME];
            if (request.Uri.Scheme == "sips" || (route != null && route.First.Uri.Scheme == "sips"))
            {
                if (request.Contact.Uri.Scheme != "sips")
                    throw new BadRequestException("Contact must use 'sips' if Request URI or first VIA uses 'sips'");
            }

            // When a UAC receives a response that establishes a dialog, it
            // constructs the state of the dialog.  This state MUST be maintained
            // for the duration of the dialog.
            var dialog = new Dialog();
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
                                  ? CopyAndReverse((Route) response.Headers[Route.RECORD_ROUTE_LNAME])
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

            dialog.Id = response.CallId + "-" + (response.From.Parameters["tag"] ?? string.Empty) + "-" +
                        (response.To.Parameters["tag"] ?? string.Empty);

            dialog.Terminated += OnDialogTerminated;

            lock (_dialogs)
                _dialogs.Add(dialog.Id, dialog);

            return dialog;
        }

        private void OnDialogTerminated(object sender, EventArgs e)
        {
            Dialog dialog = (Dialog) sender;
            dialog.Terminated -= OnDialogTerminated;
            lock (_dialogs)
                _dialogs.Remove(dialog.Id);
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
                _logger.Debug("Request do not have a to tag, ignoring response " + response + " for request " +
                              transaction.Request);
                return null;
            }

            // See RFC3261 Section 12.1 Creation of a Dialog
            if (StatusCodeHelper.Is3456xx(response))
            {
                _logger.Warning("Error response: " + response + " to request " + transaction.Request);
                return null;
            }

            // RFC 3261 Section 12.1.2
            var dialog = new Dialog();

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

            dialog.Id = response.CallId + "-" + (response.To.Parameters["tag"] ?? string.Empty) + "-" +
                        (response.From.Parameters["tag"] ?? string.Empty);

            return dialog;
        }

        public bool Process(IRequest request, IServerTransaction transaction)
        {
            //12.2.2 UAS Behavior

            //   Requests sent within a dialog, as any other requests, are atomic.  If
            //   a particular request is accepted by the UAS, all the state changes
            //   associated with it are performed.  If the request is rejected, none
            //   of the state changes are performed.

            //      Note that some requests, such as INVITEs, affect several pieces of
            //      state.

            //   The UAS will receive the request from the transaction layer.  If the
            //   request has a tag in the To header field, the UAS core computes the
            //   dialog identifier corresponding to the request and compares it with
            //   existing dialogs.  If there is a match, this is a mid-dialog request.
            //   In that case, the UAS first applies the same processing rules for
            //   requests outside of a dialog, discussed in Section 8.2.

            //   If the request has a tag in the To header field, but the dialog
            //   identifier does not match any existing dialogs, the UAS may have
            //   crashed and restarted, or it may have received a request for a
            //   different (possibly failed) UAS (the UASs can construct the To tags
            //   so that a UAS can identify that the tag was for a UAS for which it is
            //   providing recovery).  Another possibility is that the incoming
            //   request has been simply misrouted.  Based on the To tag, the UAS MAY
            //   either accept or reject the request.  Accepting the request for
            //   acceptable To tags provides robustness, so that dialogs can persist
            //   even through crashes.  UAs wishing to support this capability must
            //   take into consideration some issues such as choosing monotonically
            //   increasing CSeq sequence numbers even across reboots, reconstructing
            //   the route set, and accepting out-of-range RTP timestamps and sequence
            //   numbers.


            string callId = request.CallId;
            string localTag = request.To.Parameters["tag"];
            string remoteTag = request.From.Parameters["tag"];
            if (callId != null && localTag != null && remoteTag != null)
            {
                string dialogId = callId + "-" + localTag + "-" + remoteTag;
                Dialog dialog;
                lock (_dialogs)
                {
                    if (_dialogs.TryGetValue(dialogId, out dialog))
                        return dialog.Process(request);
                }
            }

            if (localTag != null)
            {
                // If the UAS wishes to reject the request because it does not wish to
                // recreate the dialog, it MUST respond to the request with a 481
                // (Call/Transaction Does Not Exist) status code and pass that to the
                // server transaction.
                IResponse response = request.CreateResponse(StatusCode.CallOrTransactionDoesNotExist,
                                                            "Dialog was not found");
                _logger.Warning("Failed to find dialog with to tag: " + localTag);
                transaction.Send(response);
            }
            return true;
        }
    }
}