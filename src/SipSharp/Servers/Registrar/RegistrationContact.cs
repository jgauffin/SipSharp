using System;

namespace SipSharp.Servers.Registrar
{
    public class RegistrationContact : Contact
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="uri">The URI.</param>
        public RegistrationContact(string name, SipUri uri) : base(name, uri)
        {
        }

        /// <summary>
        /// Gets or sets call id used during registration.
        /// </summary>
        public string CallId { get; set; }

        /// <summary>
        /// Gets or sets expires time.
        /// </summary>
        public int Expires { get; set; }

        /// <summary>
        /// Gets or sets when the binding expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets contact quality.
        /// </summary>
        public double Quality { get; set; }

        /// <summary>
        /// Gets or sets sequence number from CSeq.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets when the binding was updated last.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}