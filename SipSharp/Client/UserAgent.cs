using System;
using System.Net;
using SipSharp.Calls;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;
using Authorization=SipSharp.Messages.Headers.Authorization;

namespace SipSharp.Client
{
    /// <summary>
    /// A user agent represents an end system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It contains a user agent
    /// client (UAC), which generates requests, and a user agent server
    /// (UAS), which responds to them.  A UAC is capable of generating a
    /// request based on some external stimulus (the user clicking a button,
    /// or a signal on a PSTN line) and processing a response.  A UAS is
    /// capable of receiving a request and generating a response based on
    /// user input, external stimulus, the result of a program execution, or
    /// some other mechanism.
    /// </para><para>
    /// When a UAC sends a request, the request passes through some number of
    /// proxy servers, which forward the request towards the UAS. When the
    /// UAS generates a response, the response is forwarded towards the UAC.
    /// </para><para>
    /// UAC and UAS procedures depend strongly on two factors.  First, based
    /// on whether the request or response is inside or outside of a dialog,
    /// and second, based on the method of a request.  Dialogs are discussed
    /// thoroughly in Section 12; they represent a peer-to-peer relationship
    /// between user agents and are established by specific SIP methods, such
    /// as INVITE.
    /// </para>
    /// <para>
    /// Security procedures for requests and responses outside of a dialog
    /// are described in RFC3261 Section 26.  Specifically, mechanisms exist for the
    /// UAS and UAC to mutually authenticate.  A limited set of privacy
    /// features are also supported through encryption of bodies using
    /// S/MIME.
    /// </para>
    /// </remarks>
    public class UserAgent
    {
        private readonly MethodHandlers _handlers = new MethodHandlers();
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (UserAgent));
        private readonly UserAgentServer _server;
        private readonly ISipStack _stack;
        private UserAgentClient _client;
        private NetworkCredential _credentials;
        private IPEndPoint _endPoint;
        private int _sequenceNumber;
        private CallManager _callManager;

        private UserAgent()
        {
        }

        internal UserAgent(ISipStack stack)
        {
            _stack = stack;
            _stack.RequestReceived += OnRequest;
            _stack.ResponseReceived += OnInviteResponse;
        }


        /// <summary>
        /// Call someone
        /// </summary>
        /// <param name="contact"></param>
        public void MakeCall(Contact contact)
        {
            IRequest request = CreateRequest("INVITE", _contact, contact);
            IClientTransaction transaction = _stack.CreateClientTransaction(request);
            transaction.ResponseReceived += OnInviteResponse;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            _server.OnRequest(sender, e);
        }

        /// <summary>
        /// Responses are first processed by the transport layer and then passed
        /// up to the transaction layer.  The transaction layer performs its
        /// processing and then passes the response up to the TU.  The majority
        /// of response processing in the TU is method specific.  However, there
        /// are some general behaviors independent of the method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInviteResponse(object sender, ResponseEventArgs e)
        {
            var transaction = (IClientTransaction) sender;

            // RFC3261 Section 8.3.1.1
            // In some cases, the response returned by the transaction layer will
            // not be a SIP message, but rather a transaction layer error.  When a
            // timeout error is received from the transaction layer, it MUST be
            // treated as if a 408 (Request Timeout) status code has been received.
            // If a fatal transport error is reported by the transport layer
            // (generally, due to fatal ICMP errors in UDP or connection failures in
            // TCP), the condition MUST be treated as a 503 (Service Unavailable)
            // status code.	
            // TODO: Propagate errors from the TL in the described way.


            // RFC3261 Section 8.3.1.2 is done in SipParser.


            // RFC3261 Section 8.1.3.3 Vias
            // If more than one Via header field value is present in a response, the
            // UAC SHOULD discard the message.
            //
            //    The presence of additional Via header field values that precede
            //    the originator of the request suggests that the message was
            //    misrouted or possibly corrupted.
            if (e.Response.Via.Items.Count > 1)
            {
                _logger.Info("Dropping response (received from '" + string.Empty +
                             "'), since it contains more than one via: " + e.Response);
                return;
            }

            if (!Process3xx(e.Response))
                return;

            if (!Process4xx(transaction, e.Response))
                return;


        }

        private void OnTransactionTerminated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnTransportFailed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool Process3xx(IResponse response)
        {
            if (!StatusCodeHelper.IsRedirected(response))
                return true;

            // Upon receipt of a redirection response (for example, a 301 response
            // status code), clients SHOULD use the URI(s) in the Contact header
            // field to formulate one or more new requests based on the redirected
            // request.  This process is similar to that of a proxy recursing on a
            // 3xx class response as detailed in Sections 16.5 and 16.6.  A client
            // starts with an initial target set containing exactly one URI, the
            // Request-URI of the original request.  If a client wishes to formulate
            // new requests based on a 3xx class response to that request, it places
            // the URIs to try into the target set.  Subject to the restrictions in
            // this specification, a client can choose which Contact URIs it places
            // into the target set.  As with proxy recursion, a client processing
            // 3xx class responses MUST NOT add any given URI to the target set more
            // than once.  If the original request had a SIPS URI in the Request-
            // URI, the client MAY choose to recurse to a non-SIPS URI, but SHOULD
            // inform the user of the redirection to an insecure URI.


            //    Any new request may receive 3xx responses themselves containing
            //    the original URI as a contact.  Two locations can be configured to
            //    redirect to each other.  Placing any given URI in the target set
            //    only once prevents infinite redirection loops.

            // As the target set grows, the client MAY generate new requests to the
            // URIs in any order.  A common mechanism is to order the set by the "q"
            // parameter value from the Contact header field value.  Requests to the
            // URIs MAY be generated serially or in parallel.  One approach is to
            // process groups of decreasing q-values serially and process the URIs
            // in each q-value group in parallel.  Another is to perform only serial
            // processing in decreasing q-value order, arbitrarily choosing between
            // contacts of equal q-value.

            // If contacting an address in the list results in a failure, as defined
            // in the next paragraph, the element moves to the next address in the
            // list, until the list is exhausted.  If the list is exhausted, then
            // the request has failed.

            // Failures SHOULD be detected through failure response codes (codes
            // greater than 399); for network errors the client transaction will
            // report any transport layer failures to the transaction user.  Note
            // that some response codes (detailed in 8.1.3.5) indicate that the
            // request can be retried; requests that are reattempted should not be
            // considered failures.

            // When a failure for a particular contact address is received, the
            // client SHOULD try the next contact address.  This will involve
            // creating a new client transaction to deliver a new request.

            // In order to create a request based on a contact address in a 3xx
            // response, a UAC MUST copy the entire URI from the target set into the
            // Request-URI, except for the "method-param" and "header" URI
            // parameters (see Section 19.1.1 for a definition of these parameters).
            // It uses the "header" parameters to create header field values for the
            // new request, overwriting header field values associated with the
            // redirected request in accordance with the guidelines in Section
            // 19.1.5.

            // Note that in some instances, header fields that have been
            // communicated in the contact address may instead append to existing
            // request header fields in the original redirected request.  As a
            // general rule, if the header field can accept a comma-separated list
            // of values, then the new header field value MAY be appended to any
            // existing values in the original redirected request.  If the header
            // field does not accept multiple values, the value in the original
            // redirected request MAY be overwritten by the header field value
            // communicated in the contact address.  For example, if a contact
            // address is returned with the following value:

            //    sip:user@host?Subject=foo&Call-Info=<http://www.foo.com>

            // Then any Subject header field in the original redirected request is
            // overwritten, but the HTTP URL is merely appended to any existing
            // Call-Info header field values.

            // It is RECOMMENDED that the UAC reuse the same To, From, and Call-ID
            // used in the original redirected request, but the UAC MAY also choose
            // to update the Call-ID header field value for new requests, for
            // example.

            // Finally, once the new request has been constructed, it is sent using
            // a new client transaction, and therefore MUST have a new branch ID in
            // the top Via field as discussed in Section 8.1.1.7.

            // In all other respects, requests sent upon receipt of a redirect
            // response SHOULD re-use the header fields and bodies of the original
            // request.

            // In some instances, Contact header field values may be cached at UAC
            // temporarily or permanently depending on the status code received and
            // the presence of an expiration interval; see Sections 21.3.2 and
            // 21.3.3.
            return false;
        }

        /// <summary>
        /// RFC3261 Section 8.1.3.5 Processing 4xx Responses
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool Process4xx(IClientTransaction transaction, IResponse response)
        {
            if (!StatusCodeHelper.Is4xx(response))
                return true;

            // Certain 4xx response codes require specific UA processing,
            // independent of the method.

            // If a 401 (Unauthorized) or 407 (Proxy Authentication Required)
            // response is received, the UAC SHOULD follow the authorization
            // procedures of Section 22.2 and Section 22.3 to retry the request with
            // credentials.
            if (response.StatusCode == StatusCode.Unauthorized)
            {
                var request = (IRequest) transaction.Request.Clone();
                var authenticate = (Authenticate) response.Headers[Authenticate.WWW_NAME];

                var authorization = new Authorization();
                authorization.Nonce = authenticate.Nonce;
            }

            // If a 413 (Request Entity Too Large) response is received (Section
            // 21.4.11), the request contained a body that was longer than the UAS
            // was willing to accept.  If possible, the UAC SHOULD retry the
            // request, either omitting the body or using one of a smaller length.


            // If a 415 (Unsupported Media Type) response is received (Section
            // 21.4.13), the request contained media types not supported by the UAS.
            // The UAC SHOULD retry sending the request, this time only using
            // content with types listed in the Accept header field in the response,
            // with encodings listed in the Accept-Encoding header field in the
            // response, and with languages listed in the Accept-Language in the
            // response.

            // If a 416 (Unsupported URI Scheme) response is received (Section
            // 21.4.14), the Request-URI used a URI scheme not supported by the
            // server.  The client SHOULD retry the request, this time, using a SIP
            // URI.

            // If a 420 (Bad Extension) response is received (Section 21.4.15), the
            // request contained a Require or Proxy-Require header field listing an
            // option-tag for a feature not supported by a proxy or UAS.  The UAC
            // SHOULD retry the request, this time omitting any extensions listed in
            // the Unsupported header field in the response.

            // In all of the above cases, the request is retried by creating a new
            // request with the appropriate modifications.  This new request
            // constitutes a new transaction and SHOULD have the same value of the
            // Call-ID, To, and From of the previous request, but the CSeq should
            // contain a new sequence number that is one higher than the previous.

            // With other 4xx responses, including those yet to be defined, a retry
            // may or may not be possible depending on the method and the use case.
            _logger.Warning("Failed to handle status code: " + response);
            return false;
        }

        public void RegisterMethod(string method, EventHandler<RequestEventArgs> handler)
        {
            _handlers.Register(method, handler);
        }

        public void Send(IRequest request)
        {
            IClientTransaction transaction = _stack.CreateClientTransaction(request);
            transaction.Terminated += OnTransactionTerminated;
            transaction.TransportFailed += OnTransportFailed;
            transaction.ResponseReceived += OnInviteResponse;
            _stack.Send(request);
        }
    }
}