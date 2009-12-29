using System;
using System.Collections;
using System.Collections.Generic;

namespace SwitchSharp.DialPlans
{
    public class ActionCollection : IEnumerable<IDialPlanAction>
    {
        private readonly List<IDialPlanAction> _actions = new List<IDialPlanAction>();

        /// <summary>
        /// Checks if the last action is terminating.
        /// </summary>
        /// <remarks>
        /// Terminating actions will hangup the call, no other actions can be processed afterward.
        /// </remarks>
        public bool IsLastActionTerminating
        {
            get { return _actions.Count > 0 && _actions[_actions.Count - 1].IsTerminatingAction; }
        }

        /// <summary>
        /// Gets last action in the collection.
        /// </summary>
        public IDialPlanAction LastAction
        {
            get { return _actions.Count > 0 ? _actions[_actions.Count - 1] : null; }
        }

        /// <summary>
        /// Add a new action
        /// </summary>
        /// <param name="action">Action to add.</param>
        /// <exception cref="InvalidOperationException">Last action is terminating</exception>
        /// <exception cref="ArgumentNullException"><c>action</c> is <c>null</c>.</exception>
        public void Add(IDialPlanAction action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_actions.Count > 0 && LastAction.IsTerminatingAction)
                throw new InvalidOperationException("Last action is terminating");
        }

        #region IEnumerable<IDialPlanAction> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IDialPlanAction> GetEnumerator()
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