namespace SipSharp.Messages.Headers
{
    public class Event : IHeader
    {
        public const string NAME = "Event";

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        public Event()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="value">Other instance.</param>
        public Event(Event value)
        {
            EventType = value.EventType;
            EventId = value.EventId;
        }

        /// <summary>
        /// Gets or sets event identity.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets type of event.
        /// </summary>
        public string EventType { get; set; }

        #region IHeader Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new Event(this);
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return NAME; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public virtual bool Equals(IHeader other)
        {
            var header = other as Event;
            if (header != null)
                return header.EventId == EventId && string.Compare(EventType, header.EventType) == 0;
            return false;
        }

        #endregion
    }
}