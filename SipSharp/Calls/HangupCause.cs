namespace SipSharp.Calls
{
    /// <summary>
    /// Enumeration to specify the cause of a call hangup
    /// </summary>
    public enum HangupCause
    {
        /// <summary>
        /// The call was successfully closed
        /// </summary>
        Success,

        /// <summary>
        /// The default hangup reason
        /// </summary>
        NormalClearing,

        /// <summary>
        /// Could not reach destination due to network/router problem
        /// </summary>
        NoRouteTransitNet,

        /// <summary>
        /// Could not find a way to destination
        /// </summary>
        NoRouteDestination,

        /// <summary>
        /// No answer from destination
        /// </summary>
        NoAnswer,

        /// <summary>
        /// Destination rejected the call
        /// </summary>
        Rejected,

        /// <summary>
        /// Number have changed (destination have been forwarded)
        /// </summary>
        NumberChanged,

        /// <summary>
        /// Too many simultanious calls, the call got rejected
        /// </summary>
        Congestion,

        /// <summary>
        /// User is busy
        /// </summary>
        UserBusy
    }
}
