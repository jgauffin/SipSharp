using System;
using System.Net;
using SipSharp.Calls;
using SipSharp.Dialogs;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Servers;
using SipSharp.Transactions;
using SipSharp.Transports;

namespace SipSharp
{
    public class SipStack : ISipStack
    {
        private readonly Authenticator _authenticator;
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (SipStack));
        private readonly MessageFactory _messageFactory;

        private readonly SubscriberList<EventHandler<RequestEventArgs>> _requestSubscribers =
            new SubscriberList<EventHandler<RequestEventArgs>>();

        private readonly TransactionManager _transactionManager;
        private readonly TransportLayer _transportLayer;
        private Contact _contact;
        private DialogManager _dialogManager;
        private string _domain;
        private IPEndPoint _endPoint;
        private int _sequenceNumber;

        public SipStack()
        {
            var hf = new HeaderFactory();
            hf.AddDefaultParsers();
            _messageFactory = new MessageFactory(hf);
            _transportLayer = new TransportLayer(MessageFactory);
            _transportLayer.RequestReceived += OnRequest;
            _transportLayer.ResponseReceived += OnResponse;
            _transactionManager = new TransactionManager(_transportLayer);
            _authenticator = new Authenticator();
        }

        public void AddTransport(ITransport transport)
        {
            _transportLayer.Register(transport);
        }

        private bool CheckAuthentication(IRequest request, IServerTransaction transaction)
        {
            return true;
        }


        private string CreateBranch()
        {
            return "z9hG4bK" + Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        private string CreateCallId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        private string CreateTag()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(8);
        }

        private int GetNextSequenceNumber()
        {
            if (int.MaxValue == _sequenceNumber)
                _sequenceNumber = 0;

            ++_sequenceNumber;
            return _sequenceNumber;
        }


        /// <summary>
        /// A request was received from the transaction layer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRequest(object sender, RequestEventArgs e)
        {
            _logger.Trace("Received " + e.Request + " from " + e.RemoteEndPoint);

            if (_transactionManager.Process(e.Request))
                return;

            if (_dialogManager.Process(e.Request, e.Transaction))
                return;

            if (!CheckAuthentication(e.Request, e.Transaction))
            {
                _logger.Warning("Failed to authenticate");
                return;
            }

            // 8.2.1 Method Inspection
            // 
            //    Once a request is authenticated (or authentication is skipped), the
            //    UAS MUST inspect the method of the request.  If the UAS recognizes
            //    but does not support the method of a request, it MUST generate a 405
            //    (Method Not Allowed) response.  Procedures for generating responses
            //    are described in Section 8.2.6.  The UAS MUST also add an Allow
            //    header field to the 405 (Method Not Allowed) response.  The Allow
            //    header field MUST list the set of methods supported by the UAS
            //    generating the message.  The Allow header field is presented in
            //    Section 20.5.
            // 
            //    If the method is one supported by the server, processing continues.

            // 
            // 8.2.2 Header Inspection
            // 
            //    If a UAS does not understand a header field in a request (that is,
            //    the header field is not defined in this specification or in any
            //    supported extension), the server MUST ignore that header field and
            //    continue processing the message.  A UAS SHOULD ignore any malformed
            //    header fields that are not necessary for processing requests.
            // 
            // 8.2.2.1 To and Request-URI
            // 
            //    The To header field identifies the original recipient of the request
            //    designated by the user identified in the From field.  The original
            //    recipient may or may not be the UAS processing the request, due to
            //    call forwarding or other proxy operations.  A UAS MAY apply any
            //    policy it wishes to determine whether to accept requests when the To
            //    header field is not the identity of the UAS.  However, it is
            //    RECOMMENDED that a UAS accept requests even if they do not recognize
            //    the URI scheme (for example, a tel: URI) in the To header field, or
            //    if the To header field does not address a known or current user of
            //    this UAS.  If, on the other hand, the UAS decides to reject the
            //    request, it SHOULD generate a response with a 403 (Forbidden) status
            //    code and pass it to the server transaction for transmission.
            // 
            //    However, the Request-URI identifies the UAS that is to process the
            //    request.  If the Request-URI uses a scheme not supported by the UAS,
            //    it SHOULD reject the request with a 416 (Unsupported URI Scheme)
            //    response.  If the Request-URI does not identify an address that the
            //    UAS is willing to accept requests for, it SHOULD reject the request
            //    with a 404 (Not Found) response.  Typically, a UA that uses the
            //    REGISTER method to bind its address-of-record to a specific contact
            //    address will see requests whose Request-URI equals that contact
            //    address.  Other potential sources of received Request-URIs include
            //    the Contact header fields of requests and responses sent by the UA
            //    that establish or refresh dialogs.
            // 
            // 8.2.2.2 Merged Requests
            // 
            //    If the request has no tag in the To header field, the UAS core MUST
            //    check the request against ongoing transactions.  If the From tag,
            //    Call-ID, and CSeq exactly match those associated with an ongoing
            //    transaction, but the request does not match that transaction (based
            //    on the matching rules in Section 17.2.3), the UAS core SHOULD
            //    generate a 482 (Loop Detected) response and pass it to the server
            //    transaction.
            // 
            //       The same request has arrived at the UAS more than once, following
            //       different paths, most likely due to forking.  The UAS processes
            //       the first such request received and responds with a 482 (Loop
            //       Detected) to the rest of them.
            // 
            // 8.2.2.3 Require
            // 
            //    Assuming the UAS decides that it is the proper element to process the
            //    request, it examines the Require header field, if present.
            // 
            //    The Require header field is used by a UAC to tell a UAS about SIP
            //    extensions that the UAC expects the UAS to support in order to
            //    process the request properly.  Its format is described in Section
            //    20.32.  If a UAS does not understand an option-tag listed in a
            //    Require header field, it MUST respond by generating a response with
            //    status code 420 (Bad Extension).  The UAS MUST add an Unsupported
            //    header field, and list in it those options it does not understand
            //    amongst those in the Require header field of the request.
            // 
            //    Require and Proxy-Require MUST NOT be used in a SIP CANCEL
            //    request, or in an ACK request sent for a non-2xx response.  These
            //    header fields MUST be ignored if they are present in these requests.
            // 
            //    An ACK request for a 2xx response MUST contain only those Require and
            //    Proxy-Require values that were present in the initial request.
            // 
            //    Example:
            // 
            //       UAC->UAS:   INVITE sip:watson@bell-telephone.com SIP/2.0
            //                   Require: 100rel
            // 
            //       UAS->UAC:   SIP/2.0 420 Bad Extension
            //                   Unsupported: 100rel
            // 
            //       This behavior ensures that the client-server interaction will
            //       proceed without delay when all options are understood by both
            //       sides, and only slow down if options are not understood (as in the
            //       example above).  For a well-matched client-server pair, the
            //       interaction proceeds quickly, saving a round-trip often required
            //       by negotiation mechanisms.  In addition, it also removes ambiguity
            //       when the client requires features that the server does not
            //       understand.  Some features, such as call handling fields, are only
            //       of interest to end systems.
            // 
            // 8.2.3 Content Processing
            // 
            //    Assuming the UAS understands any extensions required by the client,
            //    the UAS examines the body of the message, and the header fields that
            //    describe it.  If there are any bodies whose type (indicated by the
            //    Content-Type), language (indicated by the Content-Language) or
            //    encoding (indicated by the Content-Encoding) are not understood, and
            //    that body part is not optional (as indicated by the Content-
            //    Disposition header field), the UAS MUST reject the request with a 415
            //    (Unsupported Media Type) response.  The response MUST contain an
            //    Accept header field listing the types of all bodies it understands,
            //    in the event the request contained bodies of types not supported by
            //    the UAS.  If the request contained content encodings not understood
            //    by the UAS, the response MUST contain an Accept-Encoding header field
            //    listing the encodings understood by the UAS.  If the request
            //    contained content with languages not understood by the UAS, the
            //    response MUST contain an Accept-Language header field indicating the
            //    languages understood by the UAS.  Beyond these checks, body handling
            //    depends on the method and type.  For further information on the
            //    processing of content-specific header fields, see Section 7.4 as well
            //    as Section 20.11 through 20.15.
            // 
            // 8.2.4 Applying Extensions
            // 
            //    A UAS that wishes to apply some extension when generating the
            //    response MUST NOT do so unless support for that extension is
            //    indicated in the Supported header field in the request.  If the
            //    desired extension is not supported, the server SHOULD rely only on
            //    baseline SIP and any other extensions supported by the client.  In
            //    rare circumstances, where the server cannot process the request
            //    without the extension, the server MAY send a 421 (Extension Required)
            //    response.  This response indicates that the proper response cannot be
            //    generated without support of a specific extension.  The needed
            //    extension(s) MUST be included in a Require header field in the
            //    response.  This behavior is NOT RECOMMENDED, as it will generally
            //    break interoperability.
            // 
            //    Any extensions applied to a non-421 response MUST be listed in a
            //    Require header field included in the response.  Of course, the server
            //    MUST NOT apply extensions not listed in the Supported header field in
            //    the request.  As a result of this, the Require header field in a
            //    response will only ever contain option tags defined in standards-
            //    track RFCs.
            // 
            // 8.2.5 Processing the Request
            // 
            //    Assuming all of the checks in the previous subsections are passed,
            //    the UAS processing becomes method-specific.  Section 10 covers the
            //    REGISTER request, Section 11 covers the OPTIONS request, Section 13
            //    covers the INVITE request, and Section 15 covers the BYE request.
            // 

            #region 8.2.6 Generating the Response

            // 
            //    When a UAS wishes to construct a response to a request, it follows
            //    the general procedures detailed in the following subsections.
            //    Additional behaviors specific to the response code in question, which
            //    are not detailed in this section, may also be required.
            // 
            //    Once all procedures associated with the creation of a response have
            //    been completed, the UAS hands the response back to the server
            //    transaction from which it received the request.
            // 
            // 8.2.6.1 Sending a Provisional Response
            // 
            //    One largely non-method-specific guideline for the generation of
            //    responses is that UASs SHOULD NOT issue a provisional response for a
            //    non-INVITE request.  Rather, UASs SHOULD generate a final response to
            //    a non-INVITE request as soon as possible.
            // 
            // 
            //    When a 100 (Trying) response is generated, any Timestamp header field
            //    present in the request MUST be copied into this 100 (Trying)
            //    response.  If there is a delay in generating the response, the UAS
            //    SHOULD add a delay value into the Timestamp value in the response.
            //    This value MUST contain the difference between the time of sending of
            //    the response and receipt of the request, measured in seconds.
            // 
            // 8.2.6.2 Headers and Tags
            // 
            //    The From field of the response MUST equal the From header field of
            //    the request.  The Call-ID header field of the response MUST equal the
            //    Call-ID header field of the request.  The CSeq header field of the
            //    response MUST equal the CSeq field of the request.  The Via header
            //    field values in the response MUST equal the Via header field values
            //    in the request and MUST maintain the same ordering.
            // 
            //    If a request contained a To tag in the request, the To header field
            //    in the response MUST equal that of the request.  However, if the To
            //    header field in the request did not contain a tag, the URI in the To
            //    header field in the response MUST equal the URI in the To header
            //    field; additionally, the UAS MUST add a tag to the To header field in
            //    the response (with the exception of the 100 (Trying) response, in
            //    which a tag MAY be present).  This serves to identify the UAS that is
            //    responding, possibly resulting in a component of a dialog ID.  The
            //    same tag MUST be used for all responses to that request, both final
            //    and provisional (again excepting the 100 (Trying)).  Procedures for
            //    the generation of tags are defined in Section 19.3.
            // 
            // 8.2.7 Stateless UAS Behavior
            // 
            //    A stateless UAS is a UAS that does not maintain transaction state.
            //    It replies to requests normally, but discards any state that would
            //    ordinarily be retained by a UAS after a response has been sent.  If a
            //    stateless UAS receives a retransmission of a request, it regenerates
            //    the response and resends it, just as if it were replying to the first
            //    instance of the request. A UAS cannot be stateless unless the request
            //    processing for that method would always result in the same response
            //    if the requests are identical. This rules out stateless registrars,
            //    for example.  Stateless UASs do not use a transaction layer; they
            //    receive requests directly from the transport layer and send responses
            //    directly to the transport layer.
            // 
            //    The stateless UAS role is needed primarily to handle unauthenticated
            //    requests for which a challenge response is issued.  If
            //    unauthenticated requests were handled statefully, then malicious
            //    floods of unauthenticated requests could create massive amounts of
            // 
            //    transaction state that might slow or completely halt call processing
            //    in a UAS, effectively creating a denial of service condition; for
            //    more information see Section 26.1.5.
            // 
            //    The most important behaviors of a stateless UAS are the following:
            // 
            //       o  A stateless UAS MUST NOT send provisional (1xx) responses.
            // 
            //       o  A stateless UAS MUST NOT retransmit responses.
            // 
            //       o  A stateless UAS MUST ignore ACK requests.
            // 
            //       o  A stateless UAS MUST ignore CANCEL requests.
            // 
            //       o  To header tags MUST be generated for responses in a stateless
            //          manner - in a manner that will generate the same tag for the
            //          same request consistently.  For information on tag construction
            //          see Section 19.3.
            // 
            //    In all other respects, a stateless UAS behaves in the same manner as
            //    a stateful UAS.  A UAS can operate in either a stateful or stateless
            //    mode for each new request.       

            #endregion

            IServerTransaction transaction = _transactionManager.CreateServerTransaction(e.Request);
            var args = new RequestEventArgs(e.Request, transaction, e.RemoteEndPoint);
            if (!_requestSubscribers.Invoke(e.Request.Method, handler => handler(this, args)))
            {
                IResponse response = e.Request.CreateResponse(StatusCode.NotImplemented, "Method not implemented");
                _transportLayer.Send(response);
            }
        }

        private void OnResponse(object sender, ResponseEventArgs e)
        {
/*
            If there are any client transactions in existence, the client
            transport uses the matching procedures of Section 17.1.3 to attempt
            to match the response to an existing transaction.  If there is a
            match, the response MUST be passed to that transaction.  Otherwise,
            the response MUST be passed to the core (whether it be stateless
            proxy, stateful proxy, or UA) for further processing.  Handling of
            these "stray" responses is dependent on the core (a proxy will
            forward them, while a UA will discard, for example). 
*/
            if (_transactionManager.Process(e.Response))
            {
                _logger.Trace("Response " + e.Response + " handled by transaction.");
            }
            else
            {
            }
        }

        /// <summary>
        /// Start stack.
        /// </summary>
        public void Start()
        {
            _transportLayer.Start();
        }

        #region ISipStack Members

        /// <summary>
        /// Create a new request
        /// </summary>
        /// <param name="method">Sip method.</param>
        /// <param name="from">Who is dialing?</param>
        /// <param name="to">Destination</param>
        /// <returns>Request object.</returns>
        /// <seealso cref="SipMethod"/>
        public IRequest CreateRequest(string method, Contact from, Contact to)
        {
            // A valid SIP request formulated by a UAC MUST, at a minimum, contain
            // the following header fields: To, From, CSeq, Call-ID, Max-Forwards,
            // and Via; all of these header fields are mandatory in all SIP
            // requests.  These six header fields are the fundamental building
            // blocks of a SIP message, as they jointly provide for most of the
            // critical message routing services including the addressing of
            // messages, the routing of responses, limiting message propagation,
            // ordering of messages, and the unique identification of transactions.
            // These header fields are in addition to the mandatory request line,
            // which contains the method, Request-URI, and SIP version.

            // The initial Request-URI of the message SHOULD be set to the value of
            // the URI in the To field.  One notable exception is the REGISTER
            // method; behavior for setting the Request-URI of REGISTER is given in
            // Section 10. 
            var request = new Request(method, from.Uri, "SIP/2.0");
            request.From = from;

            // A request outside of a dialog MUST NOT contain a To tag; the tag in
            // the To field of a request identifies the peer of the dialog.  Since
            // no dialog is established, no tag is present.
            // For further information on the To header field, see RFC 3261 Section 20.39.
            request.To = to;


            // The Call-ID header field acts as a unique identifier to group
            // together a series of messages.  It MUST be the same for all requests
            // and responses sent by either UA in a dialog.  It SHOULD be the same
            // in each registration from a UA.
            // In a new request created by a UAC outside of any dialog, the Call-ID
            // header field MUST be selected by the UAC as a globally unique
            // identifier over space and time unless overridden by method-specific
            // behavior.  All SIP UAs must have a means to guarantee that the Call-
            // ID header fields they produce will not be inadvertently generated by
            // any other UA.
            request.CallId = CreateCallId();

            // The CSeq header field serves as a way to identify and order
            // transactions.  It consists of a sequence number and a method.  The
            // method MUST match that of the request.  For non-REGISTER requests
            // outside of a dialog, the sequence number value is arbitrary.  The
            // sequence number value MUST be expressible as a 32-bit unsigned
            // integer and MUST be less than 2**31.  As long as it follows the above
            // guidelines, a client may use any mechanism it would like to select
            // CSeq header field values.
            request.CSeq = new CSeq(GetNextSequenceNumber(), method);


            request.MaxForwards = 70;


            // When the UAC creates a request, it MUST insert a Via into that
            // request.  The protocol name and protocol version in the header field
            // MUST be SIP and 2.0, respectively.  The Via header field value MUST
            // contain a branch parameter.  This parameter is used to identify the
            // transaction created by that request.  This parameter is used by both
            // the client and the server.
            //
            // The branch parameter value MUST be unique across space and time for
            // all requests sent by the UA.  The exceptions to this rule are CANCEL
            // and ACK for non-2xx responses.  As discussed below, a CANCEL request
            // will have the same value of the branch parameter as the request it
            // cancels.  As discussed in Section 17.1.1.3, an ACK for a non-2xx
            // response will also have the same branch ID as the INVITE whose
            // response it acknowledges.
            //
            //    The uniqueness property of the branch ID parameter, to facilitate
            //    its use as a transaction ID, was not part of RFC 2543.
            //
            // The branch ID inserted by an element compliant with this
            // specification MUST always begin with the characters "z9hG4bK".  These
            // 7 characters are used as a magic cookie (7 is deemed sufficient to
            // ensure that an older RFC 2543 implementation would not pick such a
            // value), so that servers receiving the request can determine
            // specification (that is, globally unique).  Beyond this requirement,
            // the precise format of the branch token is implementation-defined.
            //
            // The Via header maddr, ttl, and sent-by components will be set when
            // the request is processed by the transport layer (Section 18).
            //
            // Via processing for proxies is described in Section 16.6 Item 8 and
            // Section 16.7 Item 3.
            var viaEntry = new ViaEntry(_endPoint.Address.ToString(), CreateBranch());
            viaEntry.RportWanted = true;
            request.Via.Add(viaEntry);

            // The Contact header field provides a SIP or SIPS URI that can be used
            // to contact that specific instance of the UA for subsequent requests.
            // The Contact header field MUST be present and contain exactly one SIP
            // or SIPS URI in any request that can result in the establishment of a
            // dialog.  For the methods defined in this specification, that includes
            // only the INVITE request.  For these requests, the scope of the
            // Contact is global.  That is, the Contact header field value contains
            // the URI at which the UA would like to receive requests, and this URI
            // MUST be valid even if used in subsequent requests outside of any
            // dialogs.
            //
            // If the Request-URI or top Route header field value contains a SIPS
            // URI, the Contact header field MUST contain a SIPS URI as well.
            request.Contact = _contact;

            return request;
        }

        public IClientTransaction Send(IRequest request)
        {
            return _transactionManager.CreateClientTransaction(request);
        }


        public event EventHandler<RequestEventArgs> RequestReceived;
        public event EventHandler<ResponseEventArgs> ResponseReceived;


        public IServerTransaction CreateServerTransaction(IRequest request)
        {
            return _transactionManager.CreateServerTransaction(request);
        }

        public IClientTransaction CreateClientTransaction(IRequest request)
        {
            return _transactionManager.CreateClientTransaction(request);
        }

        public Authenticator Authenticator
        {
            get { return _authenticator; }
        }

        public MessageFactory MessageFactory
        {
            get { return _messageFactory; }
        }

        #endregion
    }
}