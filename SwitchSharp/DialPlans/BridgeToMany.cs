using System;

namespace SwitchSharp.DialPlans
{
    /// <summary>
    /// Bridge to multiple destinations
    /// </summary>
    public class BridgeToMany : IDialPlanAction
    {
        public void Add(BridgeDestination destination)
        {
        }

        /// <summary>
        /// Gets if more actions can be run after this one.
        /// </summary>
        public bool IsTerminatingAction
        {
            get { return false; }
        }
    }

    /// <summary>
    /// How to bridge
    /// </summary>
    public enum BridgeType
    {
        /// <summary>
        /// Call one at a time.
        /// </summary>
        Sequential,

        /// <summary>
        /// Call everyone at the same time.
        /// </summary>
        Simultaneously
    }
}