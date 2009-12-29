using SipSharp.Calls;

namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Used to hunt for dial plan actions.
    /// </summary>
    public class DialPlan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialPlan"/> class.
        /// </summary>
        /// <param name="call">Call that the dial plan should be created for.</param>
        public DialPlan(Call call)
        {
            Call = call;
            Actions = new ActionCollection();
        }


        /// <summary>
        /// Gets actions to invoke.
        /// </summary>
        public ActionCollection Actions { get; private set; }

        /// <summary>
        /// Gets or sets call that the dial plan should be made for.
        /// </summary>
        public Call Call { get; private set; }
    }
}