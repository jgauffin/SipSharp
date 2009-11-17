﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Servers.Registrar
{
    public class RegistrationReporsitory : IRegistrationRepository
    {
        private Dictionary<string, Registration> _registrations = new Dictionary<string, Registration>();


        /// <summary>
        /// Get registration for a user.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Registration Get(SipUri uri)
        {
            Registration registration;
            return !_registrations.TryGetValue(uri.UserName + "@" + uri.Domain, out registration) ? null : registration;
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
            return _registrations.ContainsKey(contact.Uri.UserName + "@" + contact.Uri.Domain);
        }

        public void Add(string userName, string domain)
        {
            _registrations.Add(userName + "@" + domain, new Registration());
        }
    }
}