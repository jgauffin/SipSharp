namespace SipSharp.Calls
{
    /// <summary>
    /// Call state
    /// </summary>
    /// <remarks>
    /// Trying to use as many of the TAPI LINECALLSTATE values.
    /// </remarks>
    public enum CallState
    {
        /// <summary>
        /// The call is receiving a busy tone. A busy tone indicates that the call cannot be completed. This occurs if either a circuit (trunk) or the remote party's station are in use.
        /// </summary>
        Busy,

        /// <summary>
        /// The call is a member of a conference call and is logically in the connected state.
        /// </summary>
        Conference,

        /// <summary>
        /// The call has been established and the connection is made. Information is able to flow over the call between the originating address and the destination address.
        /// </summary>
        Connected,

        /// <summary>
        /// The originator is dialing digits on the call. The dialed digits are collected by the switch.
        /// </summary>
        Dialing,

        /// <summary>
        /// The remote party has disconnected from the call.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The call exists but has not been connected. No activity exists on the call. This means that no call is currently active. A call can never transition out of the idle state.
        /// </summary>
        Idle,

        /// <summary>
        /// The call is being offered to the station, signaling the arrival of a new call. The offering state is not the same as causing a phone or computer to ring. In some environments, a call in the offering state does not ring the user until the switch instructs the line to ring. For example this state is in use when an incoming call appears on several station sets but only the primary address rings. The instruction to ring does not affect any call states.
        /// </summary>
        Offering,

        /// <summary>
        /// The call is on hold by the switch. This frees the physical line. This allows another call to use the line.
        /// </summary>
        OnHold,

        /// <summary>
        /// The call is currently on hold while it is being added to a conference.
        /// </summary>
        OnHoldPendingConference,

        /// <summary>
        /// Dialing has completed and the call is proceeding through the switch or telephone network. This occurs after dialing is complete and before the call reaches the dialed party, as indicated by ringback, busy, or answer.
        /// </summary>
        Proceeding,

        /// <summary>
        /// The station to be called has been reached, and the destination's switch is generating a ring tone back to the originator. A ringback means that the destination address is being alerted to the call.
        /// </summary>
        Ringback,

        /// <summary>
        /// The call exists, but its state is currently unknown. This may be the result of poor call progress detection by the service provider. A call state message with the call state set to unknown may also be generated to inform the TAPI DLL about a new call at a time when the actual call state of the call is not exactly known
        /// </summary>
        Unknown,
    }
}
