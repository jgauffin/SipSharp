using System;
using SipSharp.Messages.Headers;
using UriParser=SipSharp.Messages.Headers.Parsers.UriParser;

namespace SipSharp.Messages
{
    internal class Request : Message, IRequest
    {
        public Request(string method, string uri, string version)
        {
            Method = method;
            Uri = UriParser.Parse(uri);
            SipVersion = version;
            MaxForwards = -1;
        }

        public Request(string method, SipUri uri, string version)
        {
            Method = method;
            Uri = uri;
            SipVersion = version;
            MaxForwards = -1;
        }


        /// <summary>
        /// Validate all mandatory headers.
        /// </summary>
        /// <exception cref="BadRequestException">A header is invalid/missing.</exception>
        public override void Validate()
        {
            const string prefix = "Missing header: ";

            if (CSeq == null)
            {
                throw new BadRequestException(prefix + "CSeq");
            }
            if (To == null)
            {
                throw new BadRequestException(prefix + "To");
            }

            if (string.IsNullOrEmpty(CallId))
                throw new BadRequestException(prefix + "CallId");

            if (From == null)
            {
                throw new BadRequestException(prefix + "From");
            }
            if (Via == null)
            {
                throw new BadRequestException(prefix + "Via");
            }
            if (MaxForwards == 0)
            {
                throw new BadRequestException(prefix + "MaxForwards");
            }

            if (Via.First == null)
                throw new BadRequestException("No via headers in request! ");

            if (Method == SipMethod.NOTIFY)
            {
                if (Headers[SubscriptionState.NAME] == null)
                    throw new BadRequestException(prefix + SubscriptionState.NAME);

                if (Headers[Event.NAME] == null)
                    throw new BadRequestException(prefix + Event.NAME);
            }
            else if (Method == SipMethod.PUBLISH)
            {
                /*
             * For determining the type of the published event state, the EPA MUST include a
             * single Event header field in PUBLISH requests. The value of this header field
             * indicates the event package for which this request is publishing event state.
             */
                if (Headers[Event.NAME] == null)
                    throw new BadRequestException(prefix + Event.NAME);
            }

            /*
         * RFC 3261 8.1.1.8 The Contact header field MUST be present and contain exactly one SIP
         * or SIPS URI in any request that can result in the establishment of a dialog. For the
         * methods defined in this specification, that includes only the INVITE request. For these
         * requests, the scope of the Contact is global. That is, the Contact header field value
         * contains the URI at which the UA would like to receive requests, and this URI MUST be
         * valid even if used in subsequent requests outside of any dialogs.
         * 
         * If the Request-URI or top Route header field value contains a SIPS URI, the Contact
         * header field MUST contain a SIPS URI as well.
         */
            if (Method == SipMethod.INVITE
                || Method == SipMethod.SUBSCRIBE
                || Method == SipMethod.REFER)
            {
                if (Contact == null)
                {
                    // Make sure this is not a target refresh. If this is a target
                    // refresh its ok not to have a contact header. Otherwise
                    // contact header is mandatory.
                    if (To.Parameters["tag"] == null)
                        throw new BadRequestException(prefix + ContactHeader.NAME);
                }

                else if (string.Compare(Uri.Scheme, "sips", true) == 0)
                {
                    if (string.Compare(Contact.Uri.Scheme, "sips", false) != 0)
                        throw new BadRequestException("Scheme for contact should be sips:" + Contact.Uri);
                }
            }

            /*
         * Contact header is mandatory for a SIP INVITE request.
         */
            if (Contact == null
                && (Method == SipMethod.INVITE
                    || Method == SipMethod.REFER || Method == SipMethod.SUBSCRIBE))
                throw new BadRequestException("Contact Header is Mandatory for a SIP INVITE");


            if (Method != null
                && CSeq.Method != null
                && Method != CSeq.Method)
            {
                throw new BadRequestException("CSEQ method mismatch with  Request-Line ");
            }
        }

        #region IRequest Members


        /// <summary>
        /// Gets or sets requested URI.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The initial Request-URI of the message SHOULD be set to the value of the URI in
        /// the To field. One notable exception is the REGISTER method; behavior for
        /// setting the Request-URI of REGISTER is given in Section 10 (RFC 3261). It may
        /// also be undesirable for privacy reasons or convenience to set these fields to
        /// the same value (especially if the originating UA expects that the Request-URI
        /// will be changed during transit).
        /// </para>
        /// </remarks>
        public SipUri Uri { get; set; }

        /// <summary>
        /// Gets or sets requested method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets maximum number of times the request can be forwarded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Max-Forwards header field serves to limit the number of hops a request can
        /// transit on the way to its destination. It consists of an integer that is
        /// decremented by one at each hop. If the Max-Forwards value reaches 0 before the
        /// request reaches its destination, it will be rejected with a 483(Too Many Hops)
        /// error response.
        /// </para>
        /// <para>
        /// A UAC MUST insert a Max-Forwards header field into each request it originates
        /// with a value that SHOULD be 70. This number was chosen to be sufficiently large
        /// to guarantee that a request would not be dropped in any SIP network when there
        /// were no loops, but not so large as to consume proxy resources when a loop does
        /// occur. Lower values should be used with caution and only in networks where
        /// topologies are known by the UA.
        /// </para>
        /// </remarks>
        public int MaxForwards { get; set; }


        /// <summary>
        /// Gets or sets a SIP or SIPS URI that can be used to contact UA that sent the
        /// request.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The Contact header field MUST be present and contain exactly one SIP or SIPS
        /// URI in any request that can result in the establishment of a dialog. For the
        /// methods defined in this specification, that includes only the INVITE request.
        /// For these requests, the scope of the Contact is global. That is, the Contact
        /// header field value contains the URI at which the UA would like to receive
        /// requests, and this URI MUST be valid even if used in subsequent requests
        /// outside of any dialogs.
        /// </para>
        /// <para>
        /// If the Request-URI or top Route header field value contains a SIPS URI, the
        /// Contact header field MUST contain a SIPS URI as well.
        /// </para>
        /// </remarks>
        public Contact Contact { get; set; }

        /// <summary>
        /// Create a new response.
        /// </summary>
        /// <param name="code">Response status code</param>
        /// <param name="reason">Reason to why the status code was used.</param>
        /// <returns>A Created response.</returns>
        /// <exception cref="InvalidOperationException">Provisional responses is only valid for INVITE method.</exception>
        public IResponse CreateResponse(StatusCode code, string reason)
        {
            if (StatusCodeHelper.Is1xx(code) && Method != "INVITE")
                throw new InvalidOperationException("Provisional responses is only valid for INVITE method.");

            var response = new Response(SipVersion, code, reason);

            // When a 100 (Trying) response is generated, any Timestamp header field
            // present in the request MUST be copied into this 100 (Trying)
            // response.  If there is a delay in generating the response, the UAS
            // SHOULD add a delay value into the Timestamp value in the response.
            // This value MUST contain the difference between the time of sending of
            // the response and receipt of the request, measured in seconds.
            if (StatusCodeHelper.Is1xx(code) && Headers["Timestamp"] != null)
                response.Headers.Add("Timestamp", Headers["Timestamp"]);

            // The From field of the response MUST equal the From header field of
            // the request.  The Call-ID header field of the response MUST equal the
            // Call-ID header field of the request.  The CSeq header field of the
            // response MUST equal the CSeq field of the request.  The Via header
            // field values in the response MUST equal the Via header field values
            // in the request and MUST maintain the same ordering.
            response.From = From;
            response.CallId = CallId;
            response.CSeq = CSeq;
            response.Via = (Via) Via.Clone();


            // If a request contained a To tag in the request, the To header field
            // in the response MUST equal that of the request.  However, if the To
            // header field in the request did not contain a tag, the URI in the To
            // header field in the response MUST equal the URI in the To header
            // field; additionally, the UAS MUST add a tag to the To header field in
            // the response (with the exception of the 100 (Trying) response, in
            // which a tag MAY be present).  This serves to identify the UAS that is
            // responding, possibly resulting in a component of a dialog ID.  The
            // same tag MUST be used for all responses to that request, both final
            // and provisional (again excepting the 100 (Trying)).  Procedures for
            // the generation of tags are defined in Section 19.3.
            response.To = To;
            if (To.Parameters["Tag"] != null)
            {
                // RFC3261 Section 17.2.1:
                // The 100 (Trying) response is constructed
                // according to the procedures in Section 8.2.6, except that the
                // insertion of tags in the To header field of the response (when none
                // was present in the request) is downgraded from MAY to SHOULD NOT.
                if (!StatusCodeHelper.Is1xx(code))
                    response.To.Parameters.Add("Tag", Guid.NewGuid().ToString().Replace("-", string.Empty));
            }

            return response;
        }

        #endregion

        internal override void Assign(string name, IHeader header)
        {
            switch (name)
            {
                case "contact":
                    Contact = ((ContactHeader) header).FirstContact;
                    break;
                case "max-forwards":
                    MaxForwards = ((NumericHeader) header).Value;
                    break;
            }

            base.Assign(name, header);
        }

        public override string ToString()
        {
            return Method + " [" + Uri + "] " + CallId + "/" + CSeq;
        }
    }
}