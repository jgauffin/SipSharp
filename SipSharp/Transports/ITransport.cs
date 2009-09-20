using System;
using System.Net;

namespace SipSharp.Transports
{
    /// <summary>
    /// A transport is responsible of sending and receiving connections
    /// using a specific protocol and EndPoint.
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// Start transport.
        /// </summary>
        /// <param name="listenerEndPoint">Address/port that clients should connect to.</param>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        void Start(EndPoint listenerEndPoint);

        /// <summary>
        /// Send a buffer to a certain end point.
        /// </summary>
        /// <param name="endPoint">End point that the buffer should be sent to.</param>
        /// <param name="buffer">buffer to send.</param>
        /// <param name="offset">Position of first byte to send.</param>
        /// <param name="count">Number of bytes, from offset, to send.</param>
        /// <exception cref="ArgumentException"><see cref="EndPoint"/> is not of the type expected by the transport implementation</exception>
        /// <exception cref="ArgumentNullException"><c>endPoint</c> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><c>offset+count</c> is out of range.</exception>
        void Send(EndPoint endPoint, byte[] buffer, int offset, int count);

        /// <summary>
        /// Gets protocol used by this transporter.
        /// </summary>
        string Protocol { get; }


        /// <summary>
        /// Gets of protocol is message based.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Message based protocols like UDP should only receive one (and a complete) message
        /// in each receive. While packet based protocols like TCP can receive partial, complete or multiple
        /// messages in one packet.
        /// </para>
        /// <para>This property should be used to </para>
        /// </remarks>
        //string IsMessageBasedProtocl{ get;}


		/// <summary>
		/// A exception was unhandled in a worker thread.
		/// </summary>
    	event UnhandledExceptionEventHandler UnhandledException;
    }

    public class ReceivedEventArgs : EventArgs
    {
        public ReceivedEventArgs(byte[] buffer, int offset, int count)
        {
            
        }
    }
}
