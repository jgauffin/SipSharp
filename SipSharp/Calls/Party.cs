namespace SipSharp.Calls
{
    /// <summary>
    /// Information about a party in a call.
    /// </summary>
    public class CallParty
    {
        /// <summary>
        /// Gets or sets name of party.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets unformatted number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets whether party is registered in the switch
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// Gets or sets address of party.
        /// </summary>
        IPartyAddress Address { get; set; }
    }
}
