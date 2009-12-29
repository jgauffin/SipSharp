using SipSharp.Messages;

namespace SipSharp.Transports
{
    /// <summary>
    /// Responsible of transporting messages to/from end points.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The transport manager inspects messages to be able to determine
    /// where they should be send. It will then serialize the message
    /// and send it using the appropiate protocol. It will automatically
    /// create a new socket if no socket exists to the destination.
    /// </para>
    /// <para>
    /// The manager will switch protocol from UDP to TCP if the max UDP packet
    /// size is exceeded.
    /// </para>
    /// <para>
    /// Incoming messages are handled by the <see cref="MessageFactory"/>. Hook
    /// its events to be able to handle them.
    /// </para>
    /// </remarks>
    public interface ITransportLayer
    {
        /// <summary>
        /// Send a request to the remote end.
        /// </summary>
        /// <param name="request">Request to send.</param>
        /// <remarks>
        /// <para>
        /// The headers are inspected to get the destination
        /// of the request.
        /// </para>
        /// </remarks>
        void Send(IRequest request);


        void Send(IResponse response);
    }
}