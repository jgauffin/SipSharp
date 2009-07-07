namespace SipSharp
{
    /// <summary>
    /// 1xx Provisional responses, also known as informational responses,
    /// indicate that the server contacted is performing some further action
    /// and does not yet have a definitive response.  A server sends a 1xx
    /// response if it expects to take more than 200 ms to obtain a final
    /// response.  Note that 1xx responses are not transmitted reliably.
    /// They never cause the client to send an ACK.  Provisional (1xx)
    /// responses MAY contain message bodies, including session descriptions.
    /// 
    /// 2xx The request was successful.
    /// 
    /// 3xx responses give information about the user's new location, or
    /// about alternative services that might be able to satisfy the call.
    /// 
    /// 4xx responses are definite failure responses from a particular
    /// server.  The client SHOULD NOT retry the same request without
    /// modification (for example, adding appropriate authorization).
    /// However, the same request to a different server might be successful.
    /// 
    /// 5xx responses are failure responses given when a server itself has erred.
    /// 
    /// 6xx responses indicate that a server has definitive information about
    /// a particular user, not just the particular instance indicated in the
    /// Request-URI.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// This response indicates that the request has been received by the
        /// next-hop server and that some unspecified action is being taken on
        /// behalf of this call (for example, a database is being consulted).
        /// This response, like all other provisional responses, stops
        /// retransmissions of an INVITE by a UAC.  The 100 (Trying) response is
        /// different from other provisional responses, in that it is never
        /// forwarded upstream by a stateful proxy.
        /// </summary>
        Trying = 100,

        /// <summary>
        /// The UA receiving the INVITE is trying to alert the user.  This
        /// response MAY be used to initiate local ringback.
        /// </summary>
        Ringing = 180,

        /// <summary>
        /// A server MAY use this status code to indicate that the call is being
        /// forwarded to a different set of destinations.
        /// </summary>
        CallForwarded = 181,

        /// <summary>
        ///  The called party is temporarily unavailable, but the server has
        /// decided to queue the call rather than reject it.  When the callee
        /// becomes available, it will return the appropriate final status
        /// response.  The reason phrase MAY give further details about the
        /// status of the call, for example, "5 calls queued; expected waiting
        /// time is 15 minutes".  The server MAY issue several 182 (Queued)
        /// responses to update the caller about the status of the queued call.
        /// </summary>
        Queued = 182,

        /// <summary>
        /// The 183 (Session Progress) response is used to convey information
        /// about the progress of the call that is not otherwise classified.  The
        /// Reason-Phrase, header fields, or message body MAY be used to convey
        /// more details about the call progress.
        /// </summary>
        SessionProgress = 183,

        /// <summary>
        /// The request has succeeded.  The information returned with the
        /// response depends on the method used in the request.
        /// </summary>
        OK = 200,

        /// <summary>
        /// The address in the request resolved to several choices, each with its
        /// own specific location, and the user (or UA) can select a preferred
        /// communication end point and redirect its request to that location.
        /// 
        /// The response MAY include a message body containing a list of resource
        /// characteristics and location(s) from which the user or UA can choose
        /// the one most appropriate, if allowed by the Accept request header
        /// field.  However, no MIME types have been defined for this message
        /// body.
        /// 
        /// The choices SHOULD also be listed as Contact fields (Section 20.10).
        /// Unlike HTTP, the SIP response MAY contain several Contact fields or a
        /// list of addresses in a Contact field.  UAs MAY use the Contact header
        /// field value for automatic redirection or MAY ask the user to confirm
        /// a choice.  However, this specification does not define any standard
        /// for such automatic selection.
        /// 
        /// This status response is appropriate if the callee can be reached
        /// at several different locations and the server cannot or prefers
        /// not to proxy the request.
        /// </summary>
        MultipleChoices = 300,

        /// <summary>
        /// The user can no longer be found at the address in the Request-URI,
        /// and the requesting client SHOULD retry at the new address given by
        /// the Contact header field (Section 20.10).  The requestor SHOULD
        /// update any local directories, address books, and user location caches
        /// with this new value and redirect future requests to the address(es)
        /// listed.
        /// </summary>
        MovedPermanently = 301,

        /// <summary>
        /// The requesting client SHOULD retry the request at the new address(es)
        /// given by the Contact header field (Section 20.10).  The Request-URI
        /// of the new request uses the value of the Contact header field in the
        /// response.
        /// 
        /// The duration of the validity of the Contact URI can be indicated
        /// through an Expires (Section 20.19) header field or an expires
        /// parameter in the Contact header field.  Both proxies and UAs MAY
        /// cache this URI for the duration of the expiration time.  If there is
        /// no explicit expiration time, the address is only valid once for
        /// recursing, and MUST NOT be cached for future transactions.
        /// 
        /// If the URI cached from the Contact header field fails, the Request-
        /// URI from the redirected request MAY be tried again a single time.
        /// 
        /// The temporary URI may have become out-of-date sooner than the
        /// expiration time, and a new temporary URI may be available.
        /// </summary>
        MovedTemporarily = 302,

        /// <summary>
        /// The requested resource MUST be accessed through the proxy given by
        /// the Contact field.  The Contact field gives the URI of the proxy.
        /// The recipient is expected to repeat this single request via the
        /// proxy.  305 (Use Proxy) responses MUST only be generated by UASs.
        /// </summary>
        UseProxy = 305,

        /// <summary>
        /// The call was not successful, but alternative services are possible.
        /// The alternative services are described in the message body of the
        /// response.  Formats for such bodies are not defined here, and may be
        /// the subject of future standardization.
        /// </summary>
        AlternativeService = 380,

        /// <summary>
        /// The request could not be understood due to malformed syntax.  The
        /// Reason-Phrase SHOULD identify the syntax problem in more detail, for
        /// example, "Missing Call-ID header field".
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// The request requires user authentication.  This response is issued by
        /// UASs and registrars, while 407 (Proxy Authentication Required) is
        /// used by proxy servers.
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        PaymentRequired = 402,

        /// <summary>
        /// The server understood the request, but is refusing to fulfill it.
        /// Authorization will not help, and the request SHOULD NOT be repeated.
        /// </summary>
        Forbidden = 403,

        /// <summary>
        /// The server has definitive information that the user does not exist at
        /// the domain specified in the Request-URI.  This status is also
        /// returned if the domain in the Request-URI does not match any of the
        /// domains handled by the recipient of the request.
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// The method specified in the Request-Line is understood, but not
        /// allowed for the address identified by the Request-URI.
        /// 
        /// The response MUST include an Allow header field containing a list of
        /// valid methods for the indicated address.
        /// </summary>
        MethodNotAllowed = 405,

        /// <summary>
        /// The resource identified by the request is only capable of generating
        /// response entities that have content characteristics not acceptable
        /// according to the Accept header field sent in the request.
        /// </summary>
        NotAcceptableContent = 406,

        /// <summary>
        /// This code is similar to 401 (Unauthorized), but indicates that the
        /// client MUST first authenticate itself with the proxy.  SIP access
        /// authentication is explained in Sections 26 and 22.3.
        /// 
        /// This status code can be used for applications where access to the
        /// communication channel (for example, a telephony gateway) rather than
        /// the callee requires authentication.
        /// </summary>
        ProxyAuthenticationRequired = 407,

        /// <summary>
        /// The server could not produce a response within a suitable amount of
        /// time, for example, if it could not determine the location of the user
        /// in time.  The client MAY repeat the request without modifications at
        /// any later time.
        /// </summary>
        RequestTimeout = 408,

        /// <summary>
        /// The requested resource is no longer available at the server and no
        /// forwarding address is known.  This condition is expected to be
        /// considered permanent.  If the server does not know, or has no
        /// facility to determine, whether or not the condition is permanent, the
        /// status code 404 (Not Found) SHOULD be used instead.
        /// </summary>
        Gone = 410,

        /// <summary>
        /// The server is refusing to process a request because the request
        /// entity-body is larger than the server is willing or able to process.
        /// The server MAY close the connection to prevent the client from
        /// continuing the request.
        /// 
        /// If the condition is temporary, the server SHOULD include a Retry-
        /// After header field to indicate that it is temporary and after what
        /// time the client MAY try again.
        /// </summary>
        RequiestEntityTooLarge = 413,

        /// <summary>
        /// The server is refusing to service the request because the Request-URI
        /// is longer than the server is willing to interpret.
        /// </summary>
        RequestUriTooLong = 414,

        /// <summary>
        /// The server is refusing to service the request because the message
        /// body of the request is in a format not supported by the server for
        /// the requested method.  The server MUST return a list of acceptable
        /// formats using the Accept, Accept-Encoding, or Accept-Language header
        /// field, depending on the specific problem with the content.  UAC
        /// processing of this response is described in Section 8.1.3.5.
        /// </summary>
        UnsupportedMediaType = 415,

        /// <summary>
        /// The server cannot process the request because the scheme of the URI
        /// in the Request-URI is unknown to the server.  IClient processing of
        /// this response is described in Section 8.1.3.5.
        /// </summary>
        UnsupportedUriScheme = 416,

        /// <summary>
        /// The server did not understand the protocol extension specified in a
        /// Proxy-Require (Section 20.29) or Require (Section 20.32) header
        /// field.  The server MUST include a list of the unsupported extensions
        /// in an Unsupported header field in the response.  UAC processing of
        /// this response is described in Section 8.1.3.5.
        /// </summary>
        BadExtension = 420,

        /// <summary>
        /// The UAS needs a particular extension to process the request, but this
        /// extension is not listed in a Supported header field in the request.
        /// Responses with this status code MUST contain a Require header field
        /// listing the required extensions.
        /// 
        /// A UAS SHOULD NOT use this response unless it truly cannot provide any
        /// useful service to the client.  Instead, if a desirable extension is
        /// not listed in the Supported header field, servers SHOULD process the
        /// request using baseline SIP capabilities and any extensions supported
        /// by the client.
        /// </summary>
        ExtensionRequired = 421,

        /// <summary>
        /// The server is rejecting the request because the expiration time of
        /// the resource refreshed by the request is too short.  This response
        /// can be used by a registrar to reject a registration whose Contact
        /// header field expiration time was too small.  The use of this response
        /// and the related Min-Expires header field are described in Sections
        /// 10.2.8, 10.3, and 20.23.
        /// </summary>
        IntervalTooBrief = 423,

        /// <summary>
        /// The callee's end system was contacted successfully but the callee is
        /// currently unavailable (for example, is not logged in, logged in but
        /// in a state that precludes communication with the callee, or has
        /// activated the "do not disturb" feature).  The response MAY indicate a
        /// better time to call in the Retry-After header field.  The user could
        /// also be available elsewhere (unbeknownst to this server).  The reason
        /// phrase SHOULD indicate a more precise cause as to why the callee is
        /// unavailable.  This value SHOULD be settable by the UA.  Status 486
        /// (Busy Here) MAY be used to more precisely indicate a particular
        /// reason for the call failure.
        /// 
        /// This status is also returned by a redirect or proxy server that
        /// recognizes the user identified by the Request-URI, but does not
        /// currently have a valid forwarding location for that user.
        /// </summary>
        TemporarilyUnavailable = 480,

        /// <summary>
        /// This status indicates that the UAS received a request that does not
        /// match any existing dialog or transaction.
        /// </summary>
        CallOrTransactionDoesNotExist = 481,

        /// <summary>
        /// The server has detected a loop (Section 16.3 Item 4).
        /// </summary>
        LoopDetected = 482,

        /// <summary>
        /// The server received a request that contains a Max-Forwards (Section
        /// 20.22) header field with the value zero.
        /// </summary>
        TooManyHops = 483,

        /// <summary>
        /// The server received a request with a Request-URI that was incomplete.
        /// Additional information SHOULD be provided in the reason phrase.
        /// 
        ///       This status code allows overlapped dialing.  With overlapped
        ///       dialing, the client does not know the length of the dialing
        ///       string.  It sends strings of increasing lengths, prompting the
        ///       user for more input, until it no longer receives a 484 (Address
        ///       Incomplete) status response.
        /// </summary>
        AddressIncomplete = 484,

        /// <summary>
        /// The Request-URI was ambiguous.  The response MAY contain a listing of
        /// possible unambiguous addresses in Contact header fields.  Revealing
        /// alternatives can infringe on privacy of the user or the organization.
        /// It MUST be possible to configure a server to respond with status 404
        /// (Not Found) or to suppress the listing of possible choices for
        /// ambiguous Request-URIs.
        /// 
        /// Example response to a request with the Request-URI
        /// sip:lee@example.com:
        /// 
        /// SIP/2.0 485 Ambiguous
        /// Contact: Carol Lee <sip:carol.lee@example.com>
        /// Contact: Ping Lee <sip:p.lee@example.com>
        /// Contact: Lee M. Foote <sips:lee.foote@example.com>
        /// 
        /// Some email and voice mail systems provide this functionality.  A
        /// status code separate from 3xx is used since the semantics are
        /// different: for 300, it is assumed that the same person or service
        /// will be reached by the choices provided.  While an automated
        /// choice or sequential search makes sense for a 3xx response, user
        /// intervention is required for a 485 (Ambiguous) response.
        /// </summary>
        Ambiguous = 487,

        /// <summary>
        /// The callee's end system was contacted successfully, but the callee is
        /// currently not willing or able to take additional calls at this end
        /// system.  The response MAY indicate a better time to call in the
        /// Retry-After header field.  The user could also be available
        /// elsewhere, such as through a voice mail service.  Status 600 (Busy
        /// Everywhere) SHOULD be used if the client knows that no other end
        /// system will be able to accept this call.
        /// </summary>
        BusyHere = 486,

        /// <summary>
        /// The request was terminated by a BYE or CANCEL request.  This response
        /// is never returned for a CANCEL request itself.
        /// </summary>
        RequestTerminated = 487,

        /// <summary>
        /// The response has the same meaning as 606 (Not Acceptable), but only
        /// applies to the specific resource addressed by the Request-URI and the
        /// request may succeed elsewhere.
        /// 
        /// A message body containing a description of media capabilities MAY be
        /// present in the response, which is formatted according to the Accept
        /// header field in the INVITE (or application/sdp if not present), the
        /// same as a message body in a 200 (OK) response to an OPTIONS request.
        /// </summary>
        NotAcceptableHere = 488,

        /// <summary>
        /// The request was received by a UAS that had a pending request within
        /// the same dialog.  Section 14.2 describes how such "glare" situations
        /// are resolved.
        /// </summary>
        RequestPending = 491,

        /// <summary>
        /// The request was received by a UAS that contained an encrypted MIME
        /// body for which the recipient does not possess or will not provide an
        /// appropriate decryption key.  This response MAY have a single body
        /// containing an appropriate public key that should be used to encrypt
        /// MIME bodies sent to this UA.  Details of the usage of this response
        /// code can be found in Section 23.2.
        /// </summary>
        Undecipherable = 493,

        /// <summary>
        /// The server encountered an unexpected condition that prevented it from
        /// fulfilling the request.  The client MAY display the specific error
        /// condition and MAY retry the request after several seconds.
        /// 
        /// If the condition is temporary, the server MAY indicate when the
        /// client may retry the request using the Retry-After header field.
        /// </summary>
        InternalError = 500,

        /// <summary>
        /// The server does not support the functionality required to fulfill the
        /// request.  This is the appropriate response when a UAS does not
        /// recognize the request method and is not capable of supporting it for
        /// any user.  (Proxies forward all requests regardless of method.)
        /// 
        /// Note that a 405 (Method Not Allowed) is sent when the server
        /// recognizes the request method, but that method is not allowed or
        /// supported.
        /// </summary>
        NotImplemented = 501,

        /// <summary>
        /// The server, while acting as a gateway or proxy, received an invalid
        /// response from the downstream server it accessed in attempting to
        /// fulfill the request.
        /// </summary>
        BadGateway = 502,

        /// <summary>
        /// The server is temporarily unable to process the request due to a
        /// temporary overloading or maintenance of the server.  The server MAY
        /// indicate when the client should retry the request in a Retry-After
        /// header field.  If no Retry-After is given, the client MUST act as if
        /// it had received a 500 (Server Internal Error) response.
        /// 
        /// A client (proxy or UAC) receiving a 503 (Service Unavailable) SHOULD
        /// attempt to forward the request to an alternate server.  It SHOULD NOT
        /// forward any other requests to that server for the duration specified
        /// in the Retry-After header field, if present.
        /// 
        /// Servers MAY refuse the connection or drop the request instead of
        /// responding with 503 (Service Unavailable).
        /// </summary>
        ServiceUnavailable = 503,

        /// <summary>
        /// The server did not receive a timely response from an external server
        /// it accessed in attempting to process the request.  408 (Request
        /// Timeout) should be used instead if there was no response within the
        /// period specified in the Expires header field from the upstream
        /// server.
        /// </summary>
        ServerTimeout = 504,

        /// <summary>
        /// The server does not support, or refuses to support, the SIP protocol
        /// version that was used in the request.  The server is indicating that
        /// it is unable or unwilling to complete the request using the same
        /// major version as the client, other than with this error message.
        /// </summary>
        VersionNotSupported = 505,

        /// <summary>
        /// The server was unable to process the request since the message length
        /// exceeded its capabilities.
        /// </summary>
        MessageTooLarge = 513,

        /// <summary>
        /// The callee's end system was contacted successfully but the callee is
        /// busy and does not wish to take the call at this time.  The response
        /// MAY indicate a better time to call in the Retry-After header field.
        /// If the callee does not wish to reveal the reason for declining the
        /// call, the callee uses status code 603 (Decline) instead.  This status
        /// response is returned only if the client knows that no other end point
        /// (such as a voice mail system) will answer the request.  Otherwise,
        /// 486 (Busy Here) should be returned.
        /// </summary>
        BusyEverywhere = 600,

        /// <summary>
        /// The callee's machine was successfully contacted but the user
        /// explicitly does not wish to or cannot participate.  The response MAY
        /// indicate a better time to call in the Retry-After header field.  This
        /// status response is returned only if the client knows that no other
        /// end point will answer the request.
        /// </summary>
        Decline = 603,

        /// <summary>
        /// The server has authoritative information that the user indicated in
        /// the Request-URI does not exist anywhere.
        /// </summary>
        DoesNotExistAnywhere = 604,

        /// <summary>
        /// The user's agent was contacted successfully but some aspects of the
        /// session description such as the requested media, bandwidth, or
        /// addressing style were not acceptable.
        /// 
        /// A 606 (Not Acceptable) response means that the user wishes to
        /// communicate, but cannot adequately support the session described.
        /// The 606 (Not Acceptable) response MAY contain a list of reasons in a
        /// Warning header field describing why the session described cannot be
        /// supported.  Warning reason codes are listed in Section 20.43.
        /// </summary>
        NotAcceptableUA = 606
    }

    /// <summary>
    /// Helper to check kind of status code.
    /// </summary>
    public class StatusCodeHelper
    {
        /// <summary>
        /// Is is a 1xx message?
        /// </summary>
        /// <param name="code">StatusCode to check</param>
        /// <returns>true or false ;)</returns>
        public static bool Is1xx(StatusCode code)
        {
            int icode = (int)code;
            return icode >= 100 && icode < 200;
        }

        /// <summary>
        /// Is is a 1xx message?
        /// </summary>
        /// <param name="msg">Response to check</param>
        /// <returns>true or false ;)</returns>
        public static bool Is1xx(IResponse msg)
        {
            int icode = (int)msg.StatusCode;
            return icode >= 100 && icode < 200;
        }

        public static bool Is2xx(StatusCode code)
        {
            int icode = (int)code;
            return icode >= 200 && icode < 300;
        }

        public static bool Is2xx(IResponse msg)
        {
            int icode = (int)msg.StatusCode;
            return icode >= 200 && icode < 300;
        }

        /// <summary>
        /// Check if it's either a 3xx, 4xx, 5xx or 6xx response
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool Is3456xx(IResponse msg)
        {
            int icode = (int)msg.StatusCode;
            return icode >= 300 && icode < 700;

        }
    }
}
