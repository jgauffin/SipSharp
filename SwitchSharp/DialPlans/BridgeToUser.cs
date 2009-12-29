namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Bridge to a user in our database.
    /// </summary>
    public class BridgeToUser
    {
        /// <summary>
        /// Gets or sets action when user is available.
        /// </summary>
        public BridgeDestination Action { get; set; }

        /// <summary>
        /// Gets or sets action when user is busy.
        /// </summary>
        public ActionCollection BusyActions { get; set; }

        /// <summary>
        /// Gets or sets action when user is not available (presence)
        /// </summary>
        public ActionCollection NotAvailableActions { get; set; }

        /// <summary>
        /// Gets or sets action when user have no phones registered.
        /// </summary>
        public ActionCollection NotRegisteredActions { get; set; }

        /// <summary>
        /// Gets or sets action when user do not answer in specified time.
        /// </summary>
        public ActionCollection TimeoutActions { get; set; }
    }
}