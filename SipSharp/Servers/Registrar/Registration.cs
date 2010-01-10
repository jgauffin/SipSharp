using System;
using System.Collections.Generic;

namespace SipSharp.Servers.Registrar
{
    /// <summary>
    /// Contains a registration for a user.
    /// </summary>
    public class Registration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Registration"/> class.
        /// </summary>
        public Registration()
        {
            _contacts = new List<RegistrationContact>();
        }

        private List<RegistrationContact> _contacts;

        /// <summary>
        /// A list with all contacts for a user.
        /// </summary>
        public IList<RegistrationContact> Contacts
        {
            get { return _contacts; }
        }

        /// <summary>
        /// Gets or sets when the registration was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets extension.
        /// </summary>
        /// <remarks>
        /// Extension is used to locate user on internal calls, and as call id number
        /// on internal calls.
        /// </remarks>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets complete phone number, E.164 formatted.
        /// </summary>
        /// <remarks>
        /// Used to locate user on external inbound calls, and as call id number
        /// when user is making external outbound calls.
        /// </remarks>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// Gets or sets uri
        /// </summary>
        public SipUri Uri { get; set; }

        /// <summary>
        /// Gets or sets username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets authentication user name.
        /// </summary>
        public string AuthenticationUserName { get; set; }

        public RegistrationContact Find(SipUri uri)
        {
            foreach (RegistrationContact contact in Contacts)
            {
                if (contact.Uri == uri)
                    return contact;
            }

            return null;
        }

        public void ReplaceContacts(IEnumerable<RegistrationContact> contacts)
        {
            List<RegistrationContact> newcontacts = new List<RegistrationContact>();
            foreach (var contact in contacts)
                newcontacts.Add(contact);
            _contacts = newcontacts;
        }
    }
}