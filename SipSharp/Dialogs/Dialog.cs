using System;
using SipSharp.Headers;
using SipSharp.Messages;
using SipSharp.Messages.Headers;

namespace SipSharp.Dialogs
{
    public class Dialog : IDialog
    {
        /// <summary>
        /// Gets or sets dialog identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets if dialog is sent over a secure transport.
        /// </summary>
        public bool IsSecure { get; set; }

        /// <summary>
        /// Gets or sets local sequence number.
        /// </summary>
        public int LocalSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets local URI.
        /// </summary>
        public SipUri LocalUri { get; set; }

        /// <summary>
        /// Gets or sets remote sequence number.
        /// </summary>
        public int RemoteSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets remote target.
        /// </summary>
        /// <remarks>
        /// </para>
        /// This is the value of
        /// the Contact header of received Responses for target refresh Requests in
        /// this dialog when acting as an User Agent Client.
        /// </para><para>
        /// This is the value of the Contact header of received target refresh
        /// Requests Requests in this dialog when acting as an User Agent Server.
        /// </para>
        /// </remarks>
        /// 
        public ContactHeader RemoteTarget { get; set; }

        /// <summary>
        /// Gets or sets remote URI
        /// </summary>
        public SipUri RemoteUri { get; set; }

        /// <summary>
        /// Gets or sets route
        /// </summary>
        /// <remarks>
        /// The route set MUST be set to the list of URIs in the Record-Route
        /// header field from the response, taken in reverse order and preserving
        /// all URI parameters.  If no Record-Route header field is present in
        /// the response, the route set MUST be set to the empty set.  This route
        /// set, even if empty, overrides any pre-existing route set for future
        /// requests in this dialog.  
        /// </remarks>
        public Route RouteSet { get; set; }

        /// <summary>
        /// Create a new request within this dialog.
        /// </summary>
        /// <returns>Created request.</returns>
        /// <remarks>
        /// <para>Described in RFC3261 Section 12.2.1.1 "Generating the Request".</para>
        /// </remarks>
        public IRequest CreateRequest(string method)
        {
            // The URI in the To field of the request MUST be set to the remote URI
            // from the dialog state.  The tag in the To header field of the request
            // MUST be set to the remote tag of the dialog ID.  The From URI of the
            // request MUST be set to the local URI from the dialog state.  The tag
            // in the From header field of the request MUST be set to the local tag
            // of the dialog ID.  If the value of the remote or local tags is null,
            // the tag parameter MUST be omitted from the To or From header fields,
            // respectively.
            //    Usage of the URI from the To and From fields in the original
            //    request within subsequent requests is done for backwards
            //    compatibility with RFC 2543, which used the URI for dialog
            //    identification.  In this specification, only the tags are used for
            //    dialog identification.  It is expected that mandatory reflection
            //    of the original To and From URI in mid-dialog requests will be
            //    deprecated in a subsequent revision of this specification.
            var request = new Request(method, RemoteUri, "SIP/2.0");
            request.To.Uri = RemoteUri;
            request.To.Parameters["tag"] = RemoteTag;
            request.From.Uri = LocalUri;
            request.From.Parameters["tag"] = LocalTag;


            // The Call-ID of the request MUST be set to the Call-ID of the dialog.
            // Requests within a dialog MUST contain strictly monotonically
            // increasing and contiguous CSeq sequence numbers (increasing-by-one)
            // in each direction (excepting ACK and CANCEL of course, whose numbers
            // equal the requests being acknowledged or cancelled).  Therefore, if
            // the local sequence number is not empty, the value of the local
            // sequence number MUST be incremented by one, and this value MUST be
            // placed into the CSeq header field.  If the local sequence number is
            // empty, an initial value MUST be chosen using the guidelines of
            // Section 8.1.1.5.  The method field in the CSeq header field value
            // MUST match the method of the request.
            //    With a length of 32 bits, a client could generate, within a single
            //    call, one request a second for about 136 years before needing to
            //    wrap around.  The initial value of the sequence number is chosen
            //    so that subsequent requests within the same call will not wrap
            //    around.  A non-zero initial value allows clients to use a time-
            //    based initial sequence number.  A client could, for example,
            //    choose the 31 most significant bits of a 32-bit second clock as an
            //    initial sequence number.
            request.CallId = CallId;
            if (LocalSequenceNumber > 0)
            {
                LocalSequenceNumber += 1;
                request.CSeq.Method = method;
                request.CSeq.Number = LocalSequenceNumber;
            }


            // The UAC uses the remote target and route set to build the Request-URI
            // and Route header field of the request.

            // If the route set is empty, the UAC MUST place the remote target URI
            // into the Request-URI.  The UAC MUST NOT add a Route header field to
            // the request.

            if (RouteSet.Items.Count > 0)
            {
                // If the route set is not empty, and the first URI in the route set
                // contains the lr parameter (see Section 19.1.1), the UAC MUST place
                // the remote target URI into the Request-URI and MUST include a Route
                // header field containing the route set values in order, including all
                // parameters.
                if (RouteSet.First.IsLoose)
                {
                    request.Uri = RemoteTarget.FirstContact.Uri;
                    request.Headers[Route.ROUTE_NAME] = RouteSet;
                }

                    // If the route set is not empty, and its first URI does not contain the
                    // lr parameter, the UAC MUST place the first URI from the route set
                    // into the Request-URI, stripping any parameters that are not allowed
                    // in a Request-URI.  The UAC MUST add a Route header field containing
                    // the remainder of the route set values in order, including all
                    // parameters.  The UAC MUST then place the remote target URI into the
                    // Route header field as the last value.
                else
                {
                    request.Uri = RouteSet.First.Uri; // not cloning since same routing will always be used.
                    request.Uri.Parameters.Remove("method");
                    var route = new Route(Route.ROUTE_NAME);
                    request.Headers.Add(Route.ROUTE_NAME, route);
                    for (int i = 1; i < RouteSet.Items.Count; ++i)
                        route.Items.Add(RouteSet.Items[i]);

                    route.Items.Add(new RouteEntry {Uri = RemoteTarget.FirstContact.Uri});
                }
            }


            // A UAC SHOULD include a Contact header field in any target refresh
            // requests within a dialog, and unless there is a need to change it,
            // the URI SHOULD be the same as used in previous requests within the
            // dialog.  If the "secure" flag is true, that URI MUST be a SIPS URI.
            // As discussed in Section 12.2.2, a Contact header field in a target
            // refresh request updates the remote target URI.  This allows a UA to
            // provide a new contact address, should its address change during the
            // duration of the dialog.
            //request.Contact = Local

            // However, requests that are not target refresh requests do not affect
            // the remote target URI for the dialog.

            // The rest of the request is formed as described in Section 8.1.1.

            // Once the request has been constructed, the address of the server is
            // computed and the request is sent, using the same procedures for
            // requests outside of a dialog (Section 8.1.2).

            //    The procedures in Section 8.1.2 will normally result in the
            //    request being sent to the address indicated by the topmost Route
            //    header field value or the Request-URI if no Route header field is
            //    present.  Subject to certain restrictions, they allow the request
            //    to be sent to an alternate address (such as a default outbound
            //    proxy not represented in the route set).
            return request;
        }

        public bool Process(IRequest request)
        {
            return false;
        }

        #region IDialog Members

        /// <summary>
        /// Gets or sets unique identifier for the call leg.
        /// </summary>
        public string CallId { get; set; }

        /// <summary>
        /// Gets or sets remote tag.
        /// </summary>
        public string RemoteTag { get; set; }

        /// <summary>
        /// Gets or sets local tag.
        /// </summary>
        /// 
        public string LocalTag { get; set; }

        /// <summary>
        /// Gets or sets dialog state.
        /// </summary>
        public DialogState State { get; set; }

        #endregion

        /// <summary>
        /// Dialog have terminated.
        /// </summary>
        public event EventHandler Terminated = delegate { };
    }
}