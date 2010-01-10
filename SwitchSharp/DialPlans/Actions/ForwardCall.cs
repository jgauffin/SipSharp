namespace SwitchSharp.DialPlans.Actions
{
    /// <summary>
    /// Forward the call.
    /// </summary>
    public class ForwardCall : IAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialString"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        public ForwardCall(string number)
        {
            Number = number;
        }

        /// <summary>
        /// Gets number to forward to.
        /// </summary>
        public string Number { get; private set; }
    }
}