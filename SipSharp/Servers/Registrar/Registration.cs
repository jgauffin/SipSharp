using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Servers.Registrar
{
    public class Registration
    {
        public Registration()
        {
            Contacts = new List<RegistrationContact>();
        }

        /// <summary>
        /// Gets or sets when the registration was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// Gets or sets complete phone number, E.164 formatted.
        /// </summary>
        /// <remarks>
        /// Used to locate user on external inbound calls, and as call id number
        /// when user is making external outbound calls.
        /// </remarks>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets extension.
        /// </summary>
        /// <remarks>
        /// Extension is used to locate user on internal calls, and as call id number
        /// on internal calls.
        /// </remarks>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets uri
        /// </summary>
        public SipUri Uri { get; set; }

        public List<RegistrationContact> Contacts { get; private set; }

        public RegistrationContact Find(SipUri uri)
        {
            foreach (var contact in Contacts)
            {
                if (contact.Uri == uri)
                    return contact;
            }

            return null;
        }

        public void Replace(List<RegistrationContact> contacts)
        {
            lock (Contacts)
            {
                Contacts = contacts;
            }
        }
    }
}
