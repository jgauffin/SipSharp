using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class Proxy
    {
        private readonly ISipStack _stack;
        private ILogger _logger = LogFactory.CreateLogger(typeof (Proxy));

        public Proxy(ISipStack stack)
        {
            _stack = stack;
            _stack.RegisterMethod(null, OnRequest);
        }

        private void OnRequest(object sender, StackRequestEventArgs e)
        {
            if (!ValidateRequest(e.Request, e.Transaction))
                return;


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
                _stack.Send(response);
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
                _stack.Send(response);
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
                _stack.Send(response);
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
            MethodsHeader methods = request.Headers["proxy-require"] as MethodsHeader;
            if (methods != null)
            {
                foreach (var method in methods.Methods)
                {
                    if (IsMethodSupported(method)) continue;
                    IResponse response = request.CreateResponse(StatusCode.BadExtension,
                                                                  method + " is not supported.");
                    _stack.Send(response);
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
            Route route = request.Headers["route"]
            if (IsRecordRoute(request.Uri) && request.Route.GetAllValues().Length > 0)
            {
                request.RequestLine.Uri = request.Route.GetAllValues()[request.Route.GetAllValues().Length - 1].Address.Uri;
                SIP_t_AddressParam[] routes = request.Route.GetAllValues();
                route = (SIP_Uri)routes[routes.Length - 1].Address.Uri;
                request.Route.RemoveLastValue();
            }

            // Check if Route header field indicates this proxy.
            if (request.Route.GetAllValues().Length > 0)
            {
                route = (SIP_Uri)request.Route.GetTopMostValue().Address.Uri;

                // We consider loose-route always ours, because otherwise this message never reach here.
                if (route.Param_Lr)
                {
                    request.Route.RemoveTopMostValue();
                }
                // It's our route, remove it.
                else if (IsLocalRoute(route))
                {
                    request.Route.RemoveTopMostValue();
                }
            }            
        }

        private bool CheckAuthorization(IRequest request, IServerTransaction transaction)
        {
            if (IsAuthenticated(request))
                return true;

            Authorization auth = request.Headers[Authorization.PROXY_LNAME] as Authorization;
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

        /// <summary>
        /// Authenticate user.
        /// </summary>
        /// <param name="request">Request sent by user.</param>
        /// <param name="authorization">Authorization header.</param>
        /// <returns>true if authentication was successful; otherwise null.</returns>
        /// <exception cref="ForbiddenException">User may not try to authenticate anymore.</exception>
        protected virtual bool Authenticate(IRequest request, Authorization authorization)
        {
            
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
    }
}
