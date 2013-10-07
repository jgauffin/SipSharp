using SipSharp.Calls;

namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Hang up call.
    /// </summary>
    public class Hangup : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hangup"/> class.
        /// </summary>
        /// <param name="cause">Why call was hung up.</param>
        public Hangup(HangupCause cause)
        {
            Cause = cause;
        }

        /// <summary>
        /// Gets why call was hang up.
        /// </summary>
        public HangupCause Cause { get; private set; }
    }
}