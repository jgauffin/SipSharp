using System;

namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Bridge to a user in our database.
    /// </summary>
    public class BridgeToUser : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BridgeToUser"/> class.
        /// </summary>
        /// <param name="userId">User to bridge to.</param>
        public BridgeToUser(int userId)
        {
            UserId = userId;
            BusyActions = new ActionCollection();
            NotAvailableActions = new ActionCollection();
            NotRegisteredActions = new ActionCollection();
            TimeoutActions = new ActionCollection();

        }
        /// <summary>
        /// Gets or sets user to bridge to.
        /// </summary>
        public int UserId { get; private set; }

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

        /// <summary>
        /// Gets or sets number of seconds to wait before timeout action is executed.
        /// </summary>
        public int Timeout { get; set; }
    }
}