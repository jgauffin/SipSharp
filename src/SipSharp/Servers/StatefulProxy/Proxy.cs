/*using System;
using System.Collections.Generic;
using SipSharp.Headers;
using SipSharp.Logging;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;

namespace SipSharp.Servers.StatefulProxy
{
    /// <summary>
    /// Stateful proxy
    /// </summary>
    /// <remarks>
    /// <para>
    /// When stateful, a proxy is purely a SIP transaction processing engine.
    /// Its behavior is modeled here in terms of the server and client
    /// transactions defined in Section 17.  A stateful proxy has a server
    /// transaction associated with one or more client transactions by a
    /// higher layer proxy processing component (see figure 3), known as a
    /// proxy core.  An incoming request is processed by a server
    /// transaction.  Requests from the server transaction are passed to a
    /// proxy core.  The proxy core determines where to route the request,
    /// choosing one or more next-hop locations.  An outgoing request for
    /// each next-hop location is processed by its own associated client
    /// transaction.  The proxy core collects the responses from the client
    /// transactions and uses them to send responses to the server
    /// transaction.
    /// </para><para>
    /// A stateful proxy creates a new server transaction for each new
    /// request received.  Any retransmissions of the request will then be
    /// handled by that server transaction per Section 17.  The proxy core
    /// MUST behave as a UAS with respect to sending an immediate provisional
    /// on that server transaction (such as 100 Trying) as described in
    /// Section 8.2.6.  Thus, a stateful proxy SHOULD NOT generate 100
    /// (Trying) responses to non-INVITE requests.
    /// </para>
    /// </remarks>
    internal class Proxy : IRequestHandler
    {
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (Proxy));
        private readonly ISipStack _stack;

        public Proxy(ISipStack stack)
        {
            _stack = stack;
            _stack.Register(this);
        }

        /// <summary>
        /// Authenticate user.
        /// </summary>
        /// <param name="request">Request sent by user.</param>
        /// <param name="authorization">Authorization header.</param>
        /// <returns>true if authentication was successful; otherwise null.</returns>
        /// <exception cref="ForbiddenException">User may not try to authenticate anymore.</exception>
        protected virtual bool Authenticate(IRequest request, Authorization authorization)
        {
            return false;
        }

        private bool CheckAuthorization(IRequest request, IServerTransaction transaction)
        {
            if (IsAuthenticated(request))
                return true;

            var auth = request.Headers[Authorization.PROXY_LNAME] as Authorization;
            if (auth == null)
            {
                IResponse response = request.CreateResponse(StatusCode.ProxyAuthenticationRequired,
                                                            "Need to authenticate");
                transaction.Send(response);
                return false;
            }

            try
            {
                return Authenticate(request, auth);
            }
            catch (ForbiddenException err)
            {
                _logger.Debug("Failed to authenticate " + request + "\r\n" + err);
                IResponse response = request.CreateResponse(StatusCode.Forbidden,
                                                            err.Message);
                transaction.Send(response);
                return false;
            }
        }

        private bool DetermineRequestTargets(IRequest request, IServerTransaction transaction)
        {
            var targets = new List<SipUri>();

            //   Next, the proxy calculates the target(s) of the request.  The set of
            //   targets will either be predetermined by the contents of the request
            //   or will be obtained from an abstract location service.  Each target
            //   in the set is represented as a URI.
            //
            //   If the Request-URI of the request contains an maddr parameter, the
            //   Request-URI MUST be placed into the target set as the only target
            //   URI, and the proxy MUST proceed to Section 16.6.
            //
            //   If the domain of the Request-URI indicates a domain this element is
            //   not responsible for, the Request-URI MUST be placed into the target
            //   set as the only target, and the element MUST proceed to the task of
            //   Request Forwarding (Section 16.6).
            //
            //      There are many circumstances in which a proxy might receive a
            //      request for a domain it is not responsible for.  A firewall proxy
            //      handling outgoing calls (the way HTTP proxies handle outgoing
            //      requests) is an example of where this is likely to occur.
            //
            if (LookupTargets(request, transaction, targets))
            {
            }

            //   If the target set for the request has not been predetermined as
            //   described above, this implies that the element is responsible for the
            //   domain in the Request-URI, and the element MAY use whatever mechanism
            //   it desires to determine where to send the request.  Any of these
            //   mechanisms can be modeled as accessing an abstract Location Service.
            //   This may consist of obtaining information from a location service
            //   created by a SIP Registrar, reading a database, consulting a presence
            //   server, utilizing other protocols, or simply performing an
            //   algorithmic substitution on the Request-URI.  When accessing the
            //   location service constructed by a registrar, the Request-URI MUST
            //   first be canonicalized as described in Section 10.3 before being used
            //   as an index.  The output of these mechanisms is used to construct the
            //   target set.
            //
            //   If the Request-URI does not provide sufficient information for the
            //   proxy to determine the target set, it SHOULD return a 485 (Ambiguous)
            //   response.  This response SHOULD contain a Contact header field
            //   containing URIs of new addresses to be tried.  For example, an INVITE
            //   to sip:John.Smith@company.com may be ambiguous at a proxy whose
            //   location service has multiple John Smiths listed.  See Section
            //   21.4.23 for details.
            //
            //   Any information in or about the request or the current environment of
            //   the element MAY be used in the construction of the target set.  For
            //   instance, different sets may be constructed depending on contents or
            //   the presence of header fields and bodies, the time of day of the
            //   request's arrival, the interface on which the request arrived,
            //   failure of previous requests, or even the element's current level of
            //   utilization.
            //
            //   As potential targets are located through these services, their URIs
            //   are added to the target set.  Targets can only be placed in the
            //   target set once.  If a target URI is already present in the set
            //   (based on the definition of equality for the URI type), it MUST NOT
            //   be added again.
            //
            //   A proxy MUST NOT add additional targets to the target set if the
            //   Request-URI of the original request does not indicate a resource this
            //   proxy is responsible for.
            //
            //      A proxy can only change the Request-URI of a request during
            //      forwarding if it is responsible for that URI.  If the proxy is not
            //      responsible for that URI, it will not recurse on 3xx or 416
            //      responses as described below.
            //
            //   If the Request-URI of the original request indicates a resource this
            //   proxy is responsible for, the proxy MAY continue to add targets to
            //   the set after beginning Request Forwarding.  It MAY use any
            //   information obtained during that processing to determine new targets.
            //   For instance, a proxy may choose to incorporate contacts obtained in
            //   a redirect response (3xx) into the target set.  If a proxy uses a
            //   dynamic source of information while building the target set (for
            //   instance, if it consults a SIP Registrar), it SHOULD monitor that
            //   source for the duration of processing the request.  New locations
            //   SHOULD be added to the target set as they become available.  As
            //   above, any given URI MUST NOT be added to the set more than once.
            //
            //      Allowing a URI to be added to the set only once reduces
            //      unnecessary network traffic, and in the case of incorporating
            //      contacts from redirect requests prevents infinite recursion.
            //
            //   For example, a trivial location service is a "no-op", where the
            //   target URI is equal to the incoming request URI.  The request is sent
            //   to a specific next hop proxy for further processing.  During request
            //   forwarding of Section 16.6, Item 6, the identity of that next hop,
            //   expressed as a SIP or SIPS URI, is inserted as the top-most Route
            //   header field value into the request.
            //
            //   If the Request-URI indicates a resource at this proxy that does not
            //   exist, the proxy MUST return a 404 (Not Found) response.
            //
            //   If the target set remains empty after applying all of the above, the
            //   proxy MUST return an error response, which SHOULD be the 480
            //   (Temporarily Unavailable) response.
            return true;
        }

        /// <summary>
        /// Checks if the user sending the request have been authenticated previously.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        /// If you are not using authentication, alway return <c>true</c> from this method.
        /// </remarks>
        protected virtual bool IsAuthenticated(IRequest request)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsMethodSupported(string methodName)
        {
            return true;
        }

        protected virtual bool IsOurDomain(SipUri uri)
        {
            return false;
        }

        /// <summary>
        /// Check if the request belongs to us.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="transaction"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        private bool LookupTargets(IRequest request, IServerTransaction transaction, ICollection<SipUri> targets)
        {
            return false;
        }

        /// <summary>
        /// Route pre processing as described in RFC3261 Section 16.4 Route Information Preprocessing
        /// </summary>
        /// <param name="request"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool PreProcessRoutes(IRequest request, IServerTransaction transaction)
        {
            //   The proxy MUST inspect the Request-URI of the request.  If the
            //   Request-URI of the request contains a value this proxy previously
            //   placed into a Record-Route header field (see Section 16.6 item 4),
            //   the proxy MUST replace the Request-URI in the request with the last
            //   value from the Route header field, and remove that value from the
            //   Route header field.  The proxy MUST then proceed as if it received
            //   this modified request.

            //      This will only happen when the element sending the request to the
            //      proxy (which may have been an endpoint) is a strict router.  This
            //      rewrite on receive is necessary to enable backwards compatibility
            //      with those elements.  It also allows elements following this
            //      specification to preserve the Request-URI through strict-routing
            //      proxies (see Section 12.2.1.1).
            //
            //      This requirement does not obligate a proxy to keep state in order
            //      to detect URIs it previously placed in Record-Route header fields.
            //      Instead, a proxy need only place enough information in those URIs
            //      to recognize them as values it provided when they later appear.
            //
            var route = request.Headers["route"] as Route;
            if (route == null || route.Items.Count == 0)
                return true;

            foreach (RouteEntry entry in route.Items)
            {
                if (entry.Uri != request.Uri)
                    continue;

                _logger.Warning("Replacing request URI with " + route.Items[route.Items.Count - 1].Uri);
                request.Uri = route.Items[route.Items.Count - 1].Uri;
                route.Items.RemoveAt(route.Items.Count - 1);
                break;
            }


            //   If the Request-URI contains a maddr parameter, the proxy MUST check
            //   to see if its value is in the set of addresses or domains the proxy
            //   is configured to be responsible for.  If the Request-URI has a maddr
            //   parameter with a value the proxy is responsible for, and the request
            //   was received using the port and transport indicated (explicitly or by
            //   default) in the Request-URI, the proxy MUST strip the maddr and any
            //   non-default port or transport parameter and continue processing as if
            //   those values had not been present in the request.
            //
            //      A request may arrive with a maddr matching the proxy, but on a
            //      port or transport different from that indicated in the URI.  Such
            //      a request needs to be forwarded to the proxy using the indicated
            //      port and transport.
            //


            //   If the first value in the Route header field indicates this proxy,
            //   the proxy MUST remove that value from the request.
            // jg: Remove if route is loose, since they always only reach specified target.
            if (route.Items[0].IsLoose || IsOurDomain(route.Items[0].Uri))
                route.Items.RemoveAt(0);

            return true;
        }

        /// <summary>
        /// Validate request as described in RFC3261 Section 16.3 Request Validation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool ValidateRequest(IRequest request, IServerTransaction transaction)
        {
            // 1. Reasonable syntax check
            //
            //      The request MUST be well-formed enough to be handled with a server
            //      transaction.  Any components involved in the remainder of these
            //      Request Validation steps or the Request Forwarding section MUST be
            //      well-formed.  Any other components, well-formed or not, SHOULD be
            //      ignored and remain unchanged when the message is forwarded.  For
            //      instance, an element would not reject a request because of a
            //      malformed Date header field.  Likewise, a proxy would not remove a
            //      malformed Date header field before forwarding a request.
            //
            //      This protocol is designed to be extended.  Future extensions may
            //      define new methods and header fields at any time.  An element MUST
            //      NOT refuse to proxy a request because it contains a method or
            //      header field it does not know about.
            try
            {
                request.Validate();
            }
            catch (BadRequestException err)
            {
                IResponse response = request.CreateResponse(StatusCode.BadRequest, err.Message);
                transaction.Send(response);
                return false;
            }

            // 2. URI scheme check
            //
            //      If the Request-URI has a URI whose scheme is not understood by the
            //      proxy, the proxy SHOULD reject the request with a 416 (Unsupported
            //      URI Scheme) response.
            //
            if (request.Uri.Scheme != "sips" && request.Uri.Scheme != "sip")
            {
                IResponse response = request.CreateResponse(StatusCode.UnsupportedUriScheme,
                                                            "Scheme is not supported.");
                transaction.Send(response);
                return false;
            }

            // 3. Max-Forwards check
            //
            //      The Max-Forwards header field (Section 20.22) is used to limit the
            //      number of elements a SIP request can traverse.
            //
            //      If the request does not contain a Max-Forwards header field, this
            //      check is passed.
            //
            //      If the request contains a Max-Forwards header field with a field
            //      value greater than zero, the check is passed.
            //
            //      If the request contains a Max-Forwards header field with a field
            //      value of zero (0), the element MUST NOT forward the request.  If
            //      the request was for OPTIONS, the element MAY act as the final
            //      recipient and respond per Section 11.  Otherwise, the element MUST
            //      return a 483 (Too many hops) response.

            if (request.MaxForwards == 0)
            {
                IResponse response = request.CreateResponse(StatusCode.TooManyHops,
                                                            "Too many hops.");
                transaction.Send(response);
                return false;
            }
            request.MaxForwards -= 1;

            // 4. Optional Loop Detection check
            //
            //      An element MAY check for forwarding loops before forwarding a
            //      request.  If the request contains a Via header field with a sent-
            //      by value that equals a value placed into previous requests by the
            //      proxy, the request has been forwarded by this element before.  The
            //      request has either looped or is legitimately spiraling through the
            //      element.  To determine if the request has looped, the element MAY
            //      perform the branch parameter calculation described in Step 8 of
            //      Section 16.6 on this message and compare it to the parameter
            //      received in that Via header field.  If the parameters match, the
            //      request has looped.  If they differ, the request is spiraling, and
            //      processing continues.  If a loop is detected, the element MAY
            //      return a 482 (Loop Detected) response.
            //TODO: Implement

            // 5. Proxy-Require check
            //
            //      Future extensions to this protocol may introduce features that
            //      require special handling by proxies.  Endpoints will include a
            //      Proxy-Require header field in requests that use these features,
            //      telling the proxy not to process the request unless the feature is
            //      understood.
            //
            //      If the request contains a Proxy-Require header field (Section
            //      20.29) with one or more option-tags this element does not
            //      understand, the element MUST return a 420 (Bad Extension)
            //      response.  The response MUST include an Unsupported (Section
            //      20.40) header field listing those option-tags the element did not
            //      understand.
            var methods = request.Headers["proxy-require"] as MethodsHeader;
            if (methods != null)
            {
                foreach (string method in methods.Methods)
                {
                    if (IsMethodSupported(method)) continue;
                    IResponse response = request.CreateResponse(StatusCode.BadExtension,
                                                                method + " is not supported.");
                    transaction.Send(response);
                    return false;
                }
            }

            // 6. Proxy-Authorization check
            //
            //      If an element requires credentials before forwarding a request,
            //      the request MUST be inspected as described in Section 22.3.  That
            //      section also defines what the element must do if the inspection
            //      fails.
            if (!CheckAuthorization(request, transaction))
                return false;

            return true;
        }

        public ProcessingResult ProcessRequest(RequestContext context)
        {
            if (!ValidateRequest(e.Request, e.Transaction))
                return;

            if (!PreProcessRoutes(e.Request, e.Transaction))
                return;

            // RFC3261 section 16.5
            if (!DetermineRequestTargets(e.Request, e.Transaction))
                return;
        }
    }
}*/