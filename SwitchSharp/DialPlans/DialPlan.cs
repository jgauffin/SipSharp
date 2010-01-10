using System.Collections.Specialized;
using SipSharp;
using SipSharp.Calls;
using SwitchSharp.DialPlans.Actions;

namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Dial plan to execute
    /// </summary>
    public class DialPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialPlan"/> class.
        /// </summary>
        public DialPlan(Contact from, Contact to)
        {
            Caller = from;
            Destination = to;
            Actions = new ActionCollection();
        }

        /// <summary>
        /// Gets or sets who the caller is.
        /// </summary>
        /// <remarks>
        /// Always original caller.
        /// </remarks>
        public Contact Caller { get; set; }

        /// <summary>
        /// Gets or sets what the caller want to reach.
        /// </summary>
        public Contact Destination { get; set; }

        /// <summary>
        /// Gets or sets last destination (if this is a forward)
        /// </summary>
        public Contact Forwarded { get; set; }

        /// <summary>
        /// Gets actions to invoke.
        /// </summary>
        public ActionCollection Actions { get; private set; }

        /// <summary>
        /// Gets or sets abort processing and send back result.
        /// </summary>
        public bool Abort { get; set; }

        /// <summary>
        /// Context used by dial plan manager.
        /// </summary>
        internal object Context { get; set; }
    }
}