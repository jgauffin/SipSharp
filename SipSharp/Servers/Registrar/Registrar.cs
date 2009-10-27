using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SipSharp.Logging;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;
using Authorization=SipSharp.Messages.Headers.Authorization;

namespace SipSharp.Servers.Registrar
{
    public class Registrar
    {
        private readonly ISipStack _stack;
        private Authenticator _authenticator = new Authenticator();
        private IRegistrationRepository _repository;
        private ILogger _logger = LogFactory.CreateLogger(typeof (Registrar));

        /// <summary>
        /// Gets or sets domain that the users have to authenticate against.
        /// </summary>
        /// <example>
        /// sip:biloxi.com
        /// </example>
        public SipUri Domain { get; set; }

        /// <summary>
        /// Gets or sets more like a show domain.
        /// </summary>
        /// <example>
        /// biloxi.com
        /// </example>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets minimum time in seconds before user expires.
        /// </summary>
        /// <remarks>
        /// This is the default and minimum expires time. It will be used
        /// if the time suggested by client is lower, or if the client
        /// haven't suggested a time at all.
        /// </remarks>
        public int MinExpires { get; set; }


        public Registrar(ISipStack stack)
        {
            _stack = stack;
            stack.RegisterMethod("REGISTER", OnRegister);
        }

        private void OnRegister(object source, StackRequestEventArgs args)
        {
            if (args.Request.To.Uri.Scheme != "SIP" && args.Request.To.Uri.Scheme != "SIPS")
            {
                
            }
            /*
            When receiving a REGISTER request, a registrar follows these steps:

              1. The registrar inspects the Request-URI to determine whether it
                 has access to bindings for the domain identified in the
                 Request-URI.  If not, and if the server also acts as a proxy
                 server, the server SHOULD forward the request to the addressed
                 domain, following the general behavior for proxying messages
                 described in Section 16.
            */
            if (!IsOurDomain(args.Request.Uri))
            {
                ForwardRequest(args.Request);
                return;
            }

            /*
              2. To guarantee that the registrar supports any necessary
                 extensions, the registrar MUST process the Require header field
                 values as described for UASs in Section 8.2.2.
            */
            if (IsRequireOk(args.Request))
            {
                
            }

            IServerTransaction transaction = _stack.CreateServerTransaction(args.Request);
            IResponse response = args.Request.CreateResponse(StatusCode.OK, "You are REGISTERED! :)");
            response.Headers["Date"] = new StringHeader("Date", DateTime.Now.ToString("R"));

            /*
              3. A registrar SHOULD authenticate the UAC.  Mechanisms for the
                 authentication of SIP user agents are described in Section 22.
                 Registration behavior in no way overrides the generic
                 authentication framework for SIP.  If no authentication
                 mechanism is available, the registrar MAY take the From address
                 as the asserted identity of the originator of the request.
            */
            if (args.Request.Headers[Authorization.NAME] == null)
            {
                response.StatusCode = StatusCode.Unauthorized;
                response.ReasonPhrase = "You must authorize";
                IHeader wwwHeader = _authenticator.CreateWwwHeader(Realm, Domain);
                response.Headers.Add(WwwAuthenticate.NAME, wwwHeader);
                transaction.Send(response);
                return;
            }

            /*
              4. The registrar SHOULD determine if the authenticated user is
                 authorized to modify registrations for this address-of-record.
                 For example, a registrar might consult an authorization
                 database that maps user names to a list of addresses-of-record
                 for which that user has authorization to modify bindings.  If
                 the authenticated user is not authorized to modify bindings,
                 the registrar MUST return a 403 (Forbidden) and skip the
                 remaining steps.

                 In architectures that support third-party registration, one
                 entity may be responsible for updating the registrations
                 associated with multiple addresses-of-record.
            */


            /*
              5. The registrar extracts the address-of-record from the To header
                 field of the request.  If the address-of-record is not valid
                 for the domain in the Request-URI, the registrar MUST send a
                 404 (Not Found) response and skip the remaining steps.  The URI
                 MUST then be converted to a canonical form.  To do that, all
                 URI parameters MUST be removed (including the user-param), and
                 any escaped characters MUST be converted to their unescaped
                 form.  The result serves as an index into the list of bindings.
            */
            if (!UserExists(args.Request.To, args.Request.Uri))
            {
                response.StatusCode = StatusCode.NotFound;
                response.ReasonPhrase = "User was not found in '" + args.Request.Uri + "'.";
                transaction.Send(response);
                return;
            }

            /*
              6. The registrar checks whether the request contains the Contact
                 header field.  If not, it skips to the last step.  If the
                 Contact header field is present, the registrar checks if there
                 is one Contact field value that contains the special value "*"
                 and an Expires field.  If the request has additional Contact
                 fields or an expiration time other than zero, the request is
                 invalid, and the server MUST return a 400 (Invalid Request) and
                 skip the remaining steps.  If not, the registrar checks whether
                 the Call-ID agrees with the value stored for each binding.  If
                 not, it MUST remove the binding.  If it does agree, it MUST
                 remove the binding only if the CSeq in the request is higher
                 than the value stored for that binding.  Otherwise, the update
                 MUST be aborted and the request fails.
            */

            /*
              7. The registrar now processes each contact address in the Contact
                 header field in turn.  For each address, it determines the
                 expiration interval as follows:

                 -  If the field value has an "expires" parameter, that value
                    MUST be taken as the requested expiration.

                 -  If there is no such parameter, but the request has an
                    Expires header field, that value MUST be taken as the
                    requested expiration.

                 -  If there is neither, a locally-configured default value MUST
                    be taken as the requested expiration.

                 The registrar MAY choose an expiration less than the requested
                 expiration interval.  If and only if the requested expiration
                 interval is greater than zero AND smaller than one hour AND
                 less than a registrar-configured minimum, the registrar MAY
                 reject the registration with a response of 423 (Interval Too
                 Brief).  This response MUST contain a Min-Expires header field
                 that states the minimum expiration interval the registrar is
                 willing to honor.  It then skips the remaining steps.

                 Allowing the registrar to set the registration interval
                 protects it against excessively frequent registration refreshes
                 while limiting the state that it needs to maintain and
                 decreasing the likelihood of registrations going stale.  The
                 expiration interval of a registration is frequently used in the
                 creation of services.  An example is a follow-me service, where
                 the user may only be available at a terminal for a brief
                 period.  Therefore, registrars should accept brief
                 registrations; a request should only be rejected if the
                 interval is so short that the refreshes would degrade registrar
                 performance.

                 For each address, the registrar then searches the list of
                 current bindings using the URI comparison rules.  If the
                 binding does not exist, it is tentatively added.  If the
                 binding does exist, the registrar checks the Call-ID value.  If
                 the Call-ID value in the existing binding differs from the
                 Call-ID value in the request, the binding MUST be removed if
                 the expiration time is zero and updated otherwise.  If they are
                 the same, the registrar compares the CSeq value.  If the value
                 is higher than that of the existing binding, it MUST update or
                 remove the binding as above.  If not, the update MUST be
                 aborted and the request fails.

                 This algorithm ensures that out-of-order requests from the same
                 UA are ignored.

                 Each binding record records the Call-ID and CSeq values from
                 the request.

                 The binding updates MUST be committed (that is, made visible to
                 the proxy or redirect server) if and only if all binding
                 updates and additions succeed.  If any one of them fails (for
                 example, because the back-end database commit failed), the
                 request MUST fail with a 500 (Server Error) response and all
                 tentative binding updates MUST be removed.
            */
            Registration registration = _repository.Get(args.Request.To.Uri);
            if (registration == null)
            {
                _logger.Debug("Creating new registration for: " + args.Request.To);
                registration = _repository.Create(args.Request.To.Uri);
            }

            ContactHeader contactHeader = args.Request.Headers[ContactHeader.NAME] as ContactHeader;
            if (contactHeader == null)
            {
                _logger.Warning("Contact header was not specified.");
                response.ReasonPhrase = "Missing/Invalid contact header.";
                response.StatusCode = StatusCode.BadRequest;
                transaction.Send(response);
                return;
            }

            List<RegistrationContact> newContacts = new List<RegistrationContact>();
            foreach (var contact in contactHeader.Contacts)
            {
                int expires = MinExpires;
                if (contact.Parameters.Contains("expires"))
                {
                    if (!int.TryParse(contact.Parameters["expires"], out expires))
                    {
                        _logger.Warning("Invalid expires value: " + contact.Parameters["expires"]);
                        response.ReasonPhrase = "Invalid expires value on contact header.";
                        response.StatusCode = StatusCode.BadRequest;
                        transaction.Send(response);
                        return;
                    }

                    if (expires > 0 && expires < MinExpires)
                    {
                        _logger.Warning("To small expires value: " + expires);
                        response.ReasonPhrase = "Increase expires value.";
                        response.StatusCode = StatusCode.IntervalTooBrief;
                        response.Headers.Add("MinExpires", new NumericHeader("MinExpires", MinExpires));
                        transaction.Send(response);

                    }
                }
                RegistrationContact oldContact = registration.Find(contact.Uri);
                    if (oldContact != null && oldContact.CallId == args.Request.CallId
                        && args.Request.CSeq.SeqNr < oldContact.SequenceNumber)
                    {
                        response.StatusCode = StatusCode.BadRequest;
                        response.ReasonPhrase = "CSeq value in contact is out of order.";
                        transaction.Send(response);
                        return;
                    }
                    // Add new contact
                    newContacts.Add(new RegistrationContact
                                        {
                                            CallId = args.Request.CallId,
                                            Expires = expires,
                                            ExpiresAt = DateTime.Now.AddSeconds(expires),
                                            Quality = args.Request.Contact.Quality,
                                            SequenceNumber = args.Request.CSeq.SeqNr,
                                            UpdatedAt = DateTime.Now
                                        });
                }
            }

            // Now replace contacts.
            // We do it in two steps since no contacts should be updated
            // if someone fails.


            /*
              8. The registrar returns a 200 (OK) response.  The response MUST
                 contain Contact header field values enumerating all current
                 bindings.  Each Contact value MUST feature an "expires"
                 parameter indicating its expiration interval chosen by the
                 registrar.  The response SHOULD include a Date header field.
             */

            IRegistrarUser user = CreateUser();
            //NumericHeader expires = args.Request.Headers["Expires"] as NumericHeader;
            //user.Expires = expires != null ? Math.Max(MinExpires, expires.Value) : MinExpires;
        
            transaction.Send(response);
        }

        /// <summary>
        /// Check if we support everything in the Require header.
        /// </summary>
        /// <param name="request">Register request.</param>
        /// <returns><c>true</c> if supported; otherwise <c>false</c>.</returns>
        protected virtual bool IsRequireOk(IRequest request)
        {
            throw new NotImplementedException();
        }

        protected IRegistrarUser CreateUser()
        {
            return new RegistrarUser();
        }

        protected virtual void ForwardRequest(IRequest request)
        {
            
        }
        protected virtual bool IsOurDomain(SipUri uri)
        {
            return true;
        }

        protected virtual IHeader CreateAuthenticateHeader()
        {
            
        }


    }

    public interface RegistrarDataSource
    {
        bool Validate(Contact contact);
    }

    public interface IRegistrarUser
    {
        Contact Contact { get; set; }
        EndPoint EndPoint { get; set; }
        int Expires { get; set; }
        DateTime ExpiresAt { get; set; }
    }

    class RegistrarUser : IRegistrarUser
    {
        public Contact Contact { get; set; }
        public EndPoint EndPoint { get; set; }
        public int Expires { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
