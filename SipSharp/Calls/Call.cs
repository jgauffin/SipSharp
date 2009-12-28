namespace SipSharp.Calls
{
    /// <summary>
    /// Represents a call in the system.
    /// </summary>
    public class Call
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Call"/> class.
        /// </summary>
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
        public CallParty Caller { get; set; }

        /// <summary>
        /// Gets or sets current destination
        /// </summary>
        public CallParty Destination { get; set; }

        /// <summary>
        /// Gets or sets party that was originally called.
        /// </summary>
        public CallParty CalledParty { get; set; }

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
        public CallReasons Reason { get; set; }
    }

}
