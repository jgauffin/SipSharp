namespace SipSharp.Calls
{
    /// <summary>
    /// Represents a call in the system.
    /// </summary>
    public class Call
    {
        public Call()
        {
            State = CallState.Proceeding;
        }

        /// <summary>
        /// Gets or sets call id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets related call
        /// </summary>
        public Call RelatedCall { get; set; }

        /// <summary>
        /// Gets or sets party that made the call
        /// </summary>
        public Contact Caller { get; set; }

        /// <summary>
        /// Gets or sets current destination
        /// </summary>
        public Contact Destination { get; set; }

        /// <summary>
        /// Gets or sets party that was orginally called.
        /// </summary>
        public Contact CalledParty { get; set; }

        /// <summary>
        /// Gets or sets call state
        /// </summary>
        public CallState State { get; set; }

        /// <summary>
        /// Gets or sets previous call state
        /// </summary>
        public CallState PreviousState { get; set; }


        /// <summary>
        /// Gets or sets where the call is from.
        /// </summary>
        public CallOrigins Origin { get; set; }

        /// <summary>
        /// Gets or sets why the call was made.
        /// </summary>
        public CallReason Reason { get; set; }
    }

}
