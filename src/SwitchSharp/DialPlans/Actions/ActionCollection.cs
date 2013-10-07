using System;
using System.Collections;
using System.Collections.Generic;

namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Collection of dial plan actions.
    /// </summary>
    public class ActionCollection : IEnumerable<IAction>
    {
        private readonly LinkedList<IAction> _actions = new LinkedList<IAction>();

        /// <summary>
        /// Gets number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _actions.Count; }
        }

        /// <summary>
        /// Gets last action in the collection.
        /// </summary>
        public IAction LastAction
        {
            get { return _actions.Count > 0 ? _actions.Last.Value : null; }
        }

        /// <summary>
        /// Add a new action
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <exception cref="ArgumentNullException"><c>action</c> is <c>null</c>.</exception>
        public void AddLast(IAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _actions.AddLast(action);
        }

        /// <summary>
        /// Add a new action
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <exception cref="ArgumentNullException"><c>action</c> is <c>null</c>.</exception>
        public void AddFirst(IAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            _actions.AddFirst(action);
        }

        #region IEnumerable<IAction> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IAction> GetEnumerator()
        {
            return _actions.GetEnumerator();
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
}