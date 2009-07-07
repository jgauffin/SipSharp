namespace SipSharp.Dialogs
{
    /// <summary>
    /// Kind of state that a <see cref="IDialog"/> is in.
    /// </summary>
    public enum DialogState
    {
        /// <summary>
        /// Occurs when a dialog is created with a provisional response,
        /// </summary>
        Early,

        /// <summary>
        /// When a 2xx response arrives
        /// </summary>
        Confirmed,

        /// <summary>
        /// For other responses, or if no response arrives at all on that dialog, the early dialog terminates.
        /// </summary>
        Terminated
    }
}
