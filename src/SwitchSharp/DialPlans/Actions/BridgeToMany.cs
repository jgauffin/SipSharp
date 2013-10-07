using System;
using System.Collections;
using System.Collections.Generic;

namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Bridge to multiple destinations
    /// </summary>
    public class BridgeToMany : IAction, IEnumerable<IBridgeDestination>
    {
        private readonly List<IBridgeDestination> _items = new List<IBridgeDestination>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BridgeToMany"/> class.
        /// </summary>
        public BridgeToMany()
        {
            TimeoutActions = new ActionCollection();
            BusyActions = new ActionCollection();
        }

        /// <summary>
        /// Gets or sets how to bridge
        /// </summary>
        public BridgeType BridgeType { get; set; }

        /// <summary>
        /// Gets or sets number of seconds to call each destination.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Add another destination.
        /// </summary>
        /// <param name="destination">Add a destination.</param>
        public void Add(IBridgeDestination destination)
        {
            _items.Add(destination);
        }

        /// <summary>
        /// Gets actions to invoke on timeout.
        /// </summary>
        public ActionCollection TimeoutActions { get; private set; }

        /// <summary>
        /// Gets actions to invoke if all destinations are busy.
        /// </summary>
        public ActionCollection BusyActions { get; private set; }


        #region IEnumerable<IBridgeDestination> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IBridgeDestination> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
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