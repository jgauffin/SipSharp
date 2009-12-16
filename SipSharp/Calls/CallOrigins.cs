using System;

namespace SipSharp.Calls
{
    /// <summary>
    /// Describes origin of calls.
    /// </summary>
    /// <remarks>
    /// Trying to use as many of the TAPI LINECALLORIGIN values as possible.
    /// </remarks>
    [Flags]
    public enum CallOrigins
    {
        /// <summary>
        /// The call handle is for a conference call. That is, it is the application's connection to the conference bridge in the switch.
        /// </summary>
        Conference = 64,

        /// <summary>
        /// The call originated as an incoming call on an external line.
        /// </summary>
        External = 4,

        /// <summary>
        /// The call originated as an incoming call, but the service provider is unable to determine whether it came from another station on the same switch or from an external line. Service providers can use this constant only when TAPI version 1.4 or later has been negotiated. Otherwise, the service provider can substitute <see cref="Unavailable"/>.
        /// </summary>
        Inbound = 128,

        /// <summary>
        /// The call originated as an incoming call at a station internal to the same switching environment
        /// </summary>
        Internal = 2,

        /// <summary>
        /// The call originated from this station as an outgoing call
        /// </summary>
        Outbound = 1,

        /// <summary>
        /// 
        /// </summary>
        Unavailable = 32,

        /// <summary>
        /// The call origin is currently unknown but may become known later
        /// </summary>
        Unknown = 16
    }
}