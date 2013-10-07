using System;

namespace SipSharp.Calls
{
    /// <summary>
    /// Why a call was made.
    /// </summary>
    [Flags]
    public enum CallReasons
    {
        /// <summary>
        /// The call was the result of a call completion request.
        /// </summary>
        CallCompletion = 0x00000080,

        /// <summary>
        /// The call was camped on the address. Usually, it appears initially in the onhold state, and can be switched to using lineSwapHold. If an active call becomes idle, the camped-on call may change to the offering state and the device start ringing. TAPI version 2.0 and later.
        /// </summary>
        CampedOn = 0x00004000,

        /// <summary>
        /// This is a direct incoming or outgoing call.
        /// </summary>
        Direct = 0x00000001,

        /// <summary>
        /// This call was forwarded from another extension that was busy at the time of the call.
        /// </summary>
        ForwardBusy = 0x00000002,

        /// <summary>
        /// The call was forwarded from another extension that didn't answer the call after some number of rings.
        /// </summary>
        ForwardNoAnswer = 0x00000004,

        /// <summary>
        /// The call was forwarded unconditionally from another number.
        /// </summary>
        ForwardUnconditional = 0x00000008,

        /// <summary>
        /// The call intruded onto the line, either by a call completion action invoked by another station or by operator action. Depending on switch implementation, the call may appear either in the connected state, or conferenced with an existing active call on the line. 
        /// </summary>
        Intrude = 0x00001000,

        /// <summary>
        /// The call was parked on the address. Usually, it appears initially in the onhold state.
        /// </summary>
        Parked = 0x000002000,

        /// <summary>
        /// The call was picked up from another extension.
        /// </summary>
        Pickup = 0x00000010,

        /// <summary>
        /// The call is a reminder, or "recall", that the user has a call parked or on hold for potentially a long time.
        /// </summary>
        Reminder = 0x00000200,

        /// <summary>
        /// The call has been transferred from another number.
        /// </summary>
        Transfer = 0x00000100,

        /// <summary>
        /// The reason for the call is unavailable and will not become known later.
        /// </summary>
        Unavailable = 0x00000800,

        /// <summary>
        /// The reason for the call is currently unknown but may become known later.
        /// </summary>
        Unknown = 0x00000400,

        /// <summary>
        /// The call was retrieved as a parked call.
        /// </summary>
        Unpark = 0x00000020
    }
}