namespace SipSharp.Calls
{
    /// <summary>
    /// Information about a party in a call.
    /// </summary>
    public class CallParty
    {
        public Contact Contact { get; set; }


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
