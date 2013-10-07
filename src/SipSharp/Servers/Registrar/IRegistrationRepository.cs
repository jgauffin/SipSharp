using System;
using System.Collections.Generic;

namespace SipSharp.Servers.Registrar
{
    /// <summary>
    /// Registration database.
    /// </summary>
    /// <remarks>
    /// Used to store registrations somewhere.
    /// </remarks>
    public interface IRegistrationRepository
    {
 

        /// <summary>
        /// Update all contacts in a registration.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="contacts"></param>
        void UpdateContacts(Registration registration, IEnumerable<RegistrationContact> contacts);

        Registration GetByAuthentication(string realm, string userName);

        /// <summary>
        /// Get a contact
        /// </summary>
        /// <param name="uri">Uri from To or From header.</param>
        /// <returns></returns>
        Registration Get(SipUri uri);

        /// <summary>
        /// Update uri used when registering.
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// User might use a different domain than the one
        /// added to the database. Update it so that we can find the user.
        /// </remarks>
        void UpdateUri(Registration registration, SipUri uri);
    }
}