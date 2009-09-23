using System;

namespace SipSharp.Logging
{
    public class ConsoleLogFactory : ILogFactory
    {
        private static readonly ConsoleLogFactory _instance = new ConsoleLogFactory();
        private readonly ConsoleLogger _logger = new ConsoleLogger();


        private ConsoleLogFactory()
        {
        }

        public static ILogFactory Instance
        {
            get { return _instance; }
        }

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
            return _logger;
        }

        #endregion
    }
}