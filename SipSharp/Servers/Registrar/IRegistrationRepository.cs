namespace SipSharp.Servers.Registrar
{
    public interface IRegistrationRepository
    {
        /// <summary>
        /// Create a new registration object.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Registration Create(SipUri uri);

        /// <summary>
        /// Checks if a user exists.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="realm"></param>
        /// <returns></returns>
        bool Exists(Contact contact, SipUri realm);

        /// <summary>
        /// Get registration for a user.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Registration Get(SipUri uri);
    }
}