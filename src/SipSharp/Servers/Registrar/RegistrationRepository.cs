using System;
using System.Collections.Generic;
using SipSharp.Messages.Headers;

namespace SipSharp.Servers.Registrar
{
    public class RegistrationRepository: IRegistrationRepository
    {
        private readonly Dictionary<string, Registration> _authenticationIndex = new Dictionary<string, Registration>();
        private readonly Dictionary<string, Registration> _registrations = new Dictionary<string, Registration>();

        public void Add(SipUri uri, string authenticationUserName)
        {
            // using unique user names.
            var reg = new Registration();
            reg.Uri = uri;
            _authenticationIndex.Add(authenticationUserName, reg);
            _registrations.Add(uri.ToString(), reg);
        }

        #region IRegistrationRepository Members

        /// <summary>
        /// Checks if a user exists.
        /// </summary>
        /// <param name="contact">Contact in From header.</param>
        /// <param name="userName">User name used in authentication header.</param>
        /// <param name="realm">Real used in authentication.</param>
        /// <returns></returns>
        public bool Exists(Contact contact, string userName, string realm)
        {
            return true;
        }

        /// <summary>
        /// Get registration for a user.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Registration Get(string userName, string realm)
        {
            Registration registration;
            return !_authenticationIndex.TryGetValue(userName + "@" + realm, out registration) ? null : registration;
        }

        /// <summary>
        /// Update all contacts in a registration.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="contacts"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UpdateContacts(Registration registration, IEnumerable<RegistrationContact> contacts)
        {
            registration.ReplaceContacts(contacts);
        }

        public Registration GetByAuthentication(string realm, string userName)
        {
            Registration registration;
            return _authenticationIndex.TryGetValue(userName, out registration)
                       ? registration
                       : null;
            
        }

        /// <summary>
        /// Get registration for a user.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Registration Get(SipUri uri)
        {
            Registration registration;
            return _registrations.TryGetValue(uri.ToString(), out registration) ? registration : null;
        }

        /// <summary>
        /// Update uri used when registering.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// User might use a different domain than the one
        /// added to the database. Update it so that we can find the user.
        /// </remarks>
        public void UpdateUri(Registration registration, SipUri uri)
        {
            if (uri.Equals(registration.Uri))
                return;
            _registrations.Add(uri.ToString(), registration);
            _registrations.Remove(registration.Uri.ToString());
            registration.Uri = uri;
        }

        /// <summary>
        /// Create a new registration object.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Registration Create(SipUri uri)
        {
            return new Registration {Uri = uri};
        }

        /// <summary>
        /// Checks if a user exists.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="realm"></param>
        /// <returns></returns>
        public bool Exists(Contact contact, SipUri realm)
        {
            return _authenticationIndex.ContainsKey(contact.Uri.UserName + "@" + contact.Uri.Domain);
        }

        #endregion
    }
}