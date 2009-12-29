namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Bridge to somewhere.
    /// </summary>
    public class Bridge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bridge"/> class.
        /// </summary>
        /// <param name="destination">The destination.</param>
        public Bridge(BridgeDestination destination)
        {
            Destination = destination;
        }

        /// <summary>
        /// Gets action to invoke if destination is busy.
        /// </summary>
        /// <remarks>
        /// Action will be invoked if destination sends back busy response or if 
        /// presence indicates that the destination is busy.
        /// </remarks>
        public IDialPlanAction BusyAction { get; set; }

        /// <summary>
        /// Destination to bridge to.
        /// </summary>
        public BridgeDestination Destination { get; private set; }

        /// <summary>
        /// Gets action to invoke if destination do not answer.
        /// </summary>
        /// <remarks>
        /// Action will always be appended after this action in the executing dial plan.
        /// </remarks>
        public IDialPlanAction TimeoutAction { get; set; }
    }
}