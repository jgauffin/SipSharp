using System;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Subscription state
    /// </summary>
    /// <remarks>
    /// <para>Part of the Event implementation described in RFC3265.</para>
    /// </remarks>
    /// <seealso cref="Event"/>
    internal class SubscriptionState : IHeader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionState"/> class.
        /// </summary>
        public SubscriptionState()
        {
            
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionState"/> class.
        /// </summary>
        /// <param name="state">State to copy.</param>
        private SubscriptionState(SubscriptionState state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            Expires = state.Expires;
            Reason = state.Reason;
            RetryAfter = state.RetryAfter;
            State = state.State;
        }

        #region SubscribeReason enum

        public enum SubscribeReason
        {
            Unknown,
            /// <summary>
            /// The subscription has been terminated, but the subscriber
            /// SHOULD retry immediately with a new subscription.  One primary use
            /// of such a status code is to allow migration of subscriptions
            /// between nodes.  The "retry-after" parameter has no semantics for
            /// "deactivated".
            /// </summary>
            Deactivated,
            /// <summary>
            /// The subscription has been terminated, but the client
            /// SHOULD retry at some later time.  If a "retry-after" parameter is
            /// also present, the client SHOULD wait at least the number of
            /// seconds specified by that parameter before attempting to re-
            /// subscribe.
            /// </summary>
            Probation,

            /// <summary>
            /// The subscription has been terminated due to change in
            /// authorization policy.  Clients SHOULD NOT attempt to re-subscribe.
            /// The "retry-after" parameter has no semantics for "rejected".
            /// </summary>
            Rejected,

            /// <summary>
            /// The subscription has been terminated because it was not
            /// refreshed before it expired.  Clients MAY re-subscribe
            /// immediately.  The "retry-after" parameter has no semantics for
            /// "timeout".
            /// </summary>
            Timeout,

            /// <summary>
            /// The subscription has been terminated because the notifier
            /// could not obtain authorization in a timely fashion.  If a "retry-
            /// after" parameter is also present, the client SHOULD wait at least
            /// the number of seconds specified by that parameter before
            /// attempting to re-subscribe; otherwise, the client MAY retry
            /// immediately, but will likely get put back into pending state.
            /// </summary>
            Giveup,

            /// <summary>
            /// The subscription has been terminated because the resource
            /// state which was being monitored no longer exists.  Clients SHOULD
            /// NOT attempt to re-subscribe.  The "retry-after" parameter has no
            /// semantics for "noresource".
            /// </summary>
            NoResource,
        }

        #endregion

        #region SubscribeState enum

        public enum SubscribeState
        {
            /// <summary>
            /// Subscription has been accepted and (in general) 
            /// authorized.  If the header also contains an "expires" parameter, the
            /// subscriber SHOULD take it as the authoritative subscription duration
            /// and adjust accordingly.  The "retry-after" and "reason" parameters
            /// have no semantics for "active".
            /// </summary>
            Active,

            /// <summary>
            /// The subscription has
            /// been received by the notifier, but there is insufficient policy
            /// information to grant or deny the subscription yet.  If the header
            /// also contains an "expires" parameter, the subscriber SHOULD take it
            /// as the authoritative subscription duration and adjust accordingly.
            /// No further action is necessary on the part of the subscriber.  The
            /// "retry-after" and "reason" parameters have no semantics for
            /// "pending".
            /// </summary>
            Pending,

            /// <summary>
            /// the subscriber
            /// should consider the subscription terminated.  The "expires" parameter
            /// has no semantics for "terminated".  If a reason code is present, the
            /// client should behave as described below.  If no reason code or an
            /// unknown reason code is present, the client MAY attempt to re-
            /// subscribe at any time (unless a "retry-after" parameter is present,
            /// in which case the client SHOULD NOT attempt re-subscription until
            /// after the number of seconds specified by the "retry-after"
            /// parameter). 
            /// </summary>
            Terminated
        }

        #endregion

        public const string NAME = "Subscription-State";

        /// <summary>
        /// Gets or sets number of seconds before the subscription expires.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In order to keep subscriptions effective beyond the duration
        /// communicated in the "Expires" header, subscribers need to refresh
        /// subscriptions on a periodic basis using a new SUBSCRIBE message on
        /// the same dialog as defined in SIP [1].
        ///</para><para> 
        /// If no "Expires" header is present in a SUBSCRIBE request, the implied
        /// default is defined by the event package being used.
        ///</para><para> 
        ///    200-class responses to SUBSCRIBE requests also MUST contain an
        /// "Expires" header.  The period of time in the response MAY be shorter
        /// but MUST NOT be longer than specified in the request.  The period of
        /// time in the response is the one which defines the duration of the
        /// subscription.
        ///</para><para> 
        /// An "expires" parameter on the "Contact" header has no semantics for
        /// SUBSCRIBE and is explicitly not equivalent to an "Expires" header in
        /// a SUBSCRIBE request or response.
        ///</para><para> 
        /// A natural consequence of this scheme is that a SUBSCRIBE with an
        /// "Expires" of 0 constitutes a request to unsubscribe from an event.
        /// </para>
        /// </remarks>
        /// <value>Between 0 and 2147483648.</value>
        public int Expires { get; set; }

        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public int RetryAfter { get; set; }

        #region IHeader Members

        /// <summary>
        /// Gets or sets why subscription was terminated
        /// </summary>
        public SubscribeReason Reason { get; set; }

        /// <summary>
        /// Gets or sets current subscription state
        /// </summary>
        public SubscribeState State { get; set; }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new SubscriptionState(this);
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return NAME; }
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(IHeader other)
        {
            SubscriptionState state = other as SubscriptionState;
            if (state == null)
                return false;

            return state.State == State && state.Expires == Expires && state.RetryAfter == RetryAfter;
        }
    }
}