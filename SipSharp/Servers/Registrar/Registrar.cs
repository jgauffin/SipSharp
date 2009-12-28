using System;
using System.Collections.Generic;
using System.Net;
using SipSharp.Logging;
using SipSharp.Messages;
using SipSharp.Messages.Headers;
using SipSharp.Transactions;
using Authorization=SipSharp.Messages.Headers.Authorization;

namespace SipSharp.Servers.Registrar
{
    public class Registrar
    {
        private readonly Authenticator _authenticator;
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (Registrar));
        private readonly IRegistrationRepository _repository;
        private readonly ISipStack _stack;

        public Registrar(ISipStack stack, IRegistrationRepository repository)
        {
            _stack = stack;
            _stack.RequestReceived += OnRequest;
            _repository = repository;
            _authenticator = stack.Authenticator;
            MinExpires = 600;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            if (e.Request.Method != SipMethod.REGISTER)
                return;

            if (e.Request.To.Uri.Scheme != "sip" && e.Request.To.Uri.Scheme != "sips")
            {
                IServerTransaction trans = _stack.CreateServerTransaction(e.Request);
                IResponse resp = e.Request.CreateResponse(StatusCode.BadRequest,
                                                             "Only SIP and SIPS protocols are allowed.");
                resp.Headers["Date"] = new StringHeader("Date", DateTime.Now.ToString("R"));
                trans.Send(resp);
                return;
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
            if (!IsOurDomain(e.Request.Uri))
            {
                ForwardRequest(e.Request);
                return;
            }

            /*
              2. To guarantee that the registrar supports any necessary
                 extensions, the registrar MUST process the Require header field
                 values as described for UASs in Section 8.2.2.
            */
            if (IsRequireOk(e.Request))
            {
            }

            IServerTransaction transaction = _stack.CreateServerTransaction(e.Request);
            IResponse response = e.Request.CreateResponse(StatusCode.OK, "You are REGISTERED! :)");
            response.Headers["Date"] = new StringHeader("Date", DateTime.Now.ToString("R"));

            /*
              3. A registrar SHOULD authenticate the UAC.  Mechanisms for the
                 authentication of SIP user agents are described in Section 22.
                 Registration behavior in no way overrides the generic
                 authentication framework for SIP.  If no authentication
                 mechanism is available, the registrar MAY take the From address
                 as the asserted identity of the originator of the request.
            */
            if (e.Request.Headers[Authorization.LNAME] == null)
            {
                response.StatusCode = StatusCode.Unauthorized;
                response.ReasonPhrase = "You must authorize";
                IHeader wwwHeader = _authenticator.CreateWwwHeader(Realm, Domain);
                response.Headers.Add(Authenticate.WWW_NAME, wwwHeader);
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
            if (!_repository.Exists(e.Request.To, e.Request.Uri))
            {
                response.StatusCode = StatusCode.NotFound;
                response.ReasonPhrase = "User was not found in '" + e.Request.Uri + "'.";
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
            Registration registration = _repository.Get(e.Request.To.Uri);
            if (registration == null)
            {
                _logger.Debug("Creating new registration for: " + e.Request.To);
                registration = _repository.Create(e.Request.To.Uri);
            }

            var contactHeader = e.Request.Headers[ContactHeader.LNAME] as ContactHeader;
            if (contactHeader == null)
            {
                _logger.Warning("Contact header was not specified.");
                response.ReasonPhrase = "Missing/Invalid contact header.";
                response.StatusCode = StatusCode.BadRequest;
                transaction.Send(response);
                return;
            }

            var newContacts = new List<RegistrationContact>();
            foreach (Contact contact in contactHeader.Contacts)
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
                        _logger.Warning("Too small expires value: " + expires);
                        response.ReasonPhrase = "Increase expires value.";
                        response.StatusCode = StatusCode.IntervalTooBrief;
                        response.Headers.Add("MinExpires", new NumericHeader("MinExpires", MinExpires));
                        transaction.Send(response);
                    }
                }
                RegistrationContact oldContact = registration.Find(contact.Uri);
                if (oldContact != null && oldContact.CallId == e.Request.CallId
                    && e.Request.CSeq.Number < oldContact.SequenceNumber)
                {
                    response.StatusCode = StatusCode.BadRequest;
                    response.ReasonPhrase = "CSeq value in contact is out of order.";
                    transaction.Send(response);
                    return;
                }
                // Add new contact
                newContacts.Add(new RegistrationContact(contact.Name, contact.Uri)
                                    {
                                        CallId = e.Request.CallId,
                                        Expires = expires,
                                        ExpiresAt = DateTime.Now.AddSeconds(expires),
                                        Quality = e.Request.Contact.Quality,
                                        SequenceNumber = e.Request.CSeq.Number,
                                        UpdatedAt = DateTime.Now,
                                    });
            }


            // Now replace contacts.
            // We do it in two steps since no contacts should be updated
            // if someone fails.
            registration.Replace(newContacts);

            /*
              8. The registrar returns a 200 (OK) response.  The response MUST
                 contain Contact header field values enumerating all current
                 bindings.  Each Contact value MUST feature an "expires"
                 parameter indicating its expiration interval chosen by the
                 registrar.  The response SHOULD include a Date header field.
             */
            if (response.Contact == null)
                response.Contact = new ContactHeader("Contact");
            foreach (var contact in newContacts)
            {
                Contact regContact = new Contact(string.Empty, contact.Uri);
                regContact.Parameters.Add("expires", contact.Expires.ToString());
                regContact.Parameters.Add("q", contact.Quality.ToString());
                response.Contact.Contacts.Add(regContact);
            }
            transaction.Send(response);
        }

        /// <summary>
        /// Gets or sets domain that the users have to authenticate against.
        /// </summary>
        /// <example>
        /// sip:biloxi.com
        /// </example>
        public SipUri Domain { get; set; }

        /// <summary>
        /// Gets or sets minimum time in seconds before user expires.
        /// </summary>
        /// <remarks>
        /// This is the default and minimum expires time. It will be used
        /// if the time suggested by client is lower, or if the client
        /// haven't suggested a time at all.
        /// </remarks>
        public int MinExpires { get; set; }

        /// <summary>
        /// Gets or sets more like a show domain.
        /// </summary>
        /// <example>
        /// biloxi.com
        /// </example>
        public string Realm { get; set; }

        protected virtual IHeader CreateAuthenticateHeader()
        {
            return new Authorization(Authorization.NAME);
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

        /// <summary>
        /// Check if we support everything in the Require header.
        /// </summary>
        /// <param name="request">Register request.</param>
        /// <returns><c>true</c> if supported; otherwise <c>false</c>.</returns>
        protected virtual bool IsRequireOk(IRequest request)
        {
            return true;
        }

        public Registration Get(Contact contact)
        {
            return _repository.Get(contact.Uri);
        }
    }

    public interface IRegistrarDataSource
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

    internal class RegistrarUser : IRegistrarUser
    {
        #region IRegistrarUser Members

        public Contact Contact { get; set; }
        public EndPoint EndPoint { get; set; }
        public int Expires { get; set; }
        public DateTime ExpiresAt { get; set; }

        #endregion
    }
}