using System;

namespace SipSharp.Logging
{
    /// <summary>
    /// Factory creating null logger.
    /// </summary>
    public class NullLogFactory : ILogFactory
    {
        #region ILogFactory Members

        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        /// <remarks>
        /// MUST ALWAYS return a logger. Return <see cref="NullLogWriter"/> if no logging
        /// should be used.
        /// </remarks>
        public ILogger CreateLogger(Type type)
        {
            return new NullLogWriter();
        }

        #endregion
    }
}