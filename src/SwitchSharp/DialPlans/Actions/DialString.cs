namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// A user dial string
    /// </summary>
    /// <remarks>
    /// Strings used to reach a user.
    /// </remarks>
    public class DialString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialString"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="type">kind of CallerId to use on the outgoing call.</param>
        public DialString(string number, CallerIdType type)
        {
            Value = number;
            Type = type;
        }

        /// <summary>
        /// Gets string to dial
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets kind of CallerId to use on the outgoing call.
        /// </summary>
        public CallerIdType Type { get; private set; }
    }

    /// <summary>
    /// Type of caller id number to use
    /// </summary>
    public enum CallerIdType
    {
        /// <summary>
        /// Use default caller id.
        /// </summary>
        Default,

        /// <summary>
        /// Use caller of the other leg.
        /// </summary>
        Caller,

        /// <summary>
        /// Use destination of the other leg.
        /// </summary>
        OriginalDestination,
    }
}