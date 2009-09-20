using System;

namespace SipSharp.Logging
{
    /// <summary>
    /// Factory is used to create new logs in the system.
    /// </summary>
    public static class LogFactory
    {
        private static ILogFactory _factory;

        /// <summary>
        /// Assigns log factory being used.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <exception cref="InvalidOperationException">A factory have already been assigned.</exception>
        public static void Assign(ILogFactory logFactory)
        {
            if (_factory != null && !(logFactory is NullLogFactory))
                throw new InvalidOperationException("A factory have already been assigned.");
            _factory = logFactory;
        }

        /// <summary>
        /// Create a new logger.
        /// </summary>
        /// <param name="type">Type that requested a logger.</param>
        /// <returns>Logger for the specified type;</returns>
        public static ILogger CreateLogger(Type type)
        {
            return _factory.CreateLogger(type);
        }
    }
}