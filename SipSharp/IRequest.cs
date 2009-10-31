using System;
using System.Collections.Generic;
using SipSharp.Headers;
using SipSharp.Messages.Headers;

namespace SipSharp
{
    /// <summary>
    /// Incoming SIP request.
    /// </summary>
    /// <remarks>
    /// A valid SIP request formulated by a UAC MUST, at a minimum, contain the
    /// following header fields: To, From, CSeq, Call-ID, Max-Forwards, and Via; all of
    /// these header fields are mandatory in all SIP requests. These six header fields
    /// are the fundamental building blocks of a SIP message, as they jointly provide
    /// for most of the critical message routing services including the addressing of
    /// messages, the routing of responses, limiting message propagation, ordering of
    /// messages, and the unique identification of transactions. These header fields
    /// are in addition to the mandatory request line, which contains the method,
    /// Request-URI, and SIP version.
    /// </remarks>
    public interface IRequest : IMessage
    {
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
        SipUri Uri { get; set; }

        /// <summary>
        /// Gets or sets requested method.
        /// </summary>
        string Method { get; set; }


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
        /// <value>
        /// Not specified = -1
        /// </value>
        int MaxForwards { get; set; }

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
        Contact Contact { get; set; }

    	/// <summary>
    	/// Create a new response.
    	/// </summary>
    	/// <param name="code">Response status code</param>
    	/// <param name="reason">Reason to why the status code was used.</param>
    	/// <returns>A Created response.</returns>
    	/// <exception cref="InvalidOperationException">Provisional responses is only valid for INVITE method.</exception>
    	IResponse CreateResponse(StatusCode code, string reason);

    }
}
