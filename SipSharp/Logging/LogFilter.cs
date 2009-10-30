using System;
using System.Collections.Generic;
using SipSharp.Messages;

namespace SipSharp.Logging
{
    /// <summary>
    /// Default log filter implementation.
    /// </summary>
    public class LogFilter : ILogFilter
    {
        private readonly List<NamespaceFilter> _namespaces = new List<NamespaceFilter>();
        private readonly Dictionary<Type, LogLevel> _types = new Dictionary<Type, LogLevel>();

        /// <summary>
        /// Parser can only display errors. Transports only warnings.
        /// </summary>
        public void AddStandardRules()
        {
            AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
            AddType(typeof(SipParser), LogLevel.Error);
            AddType(typeof(MessageFactory), LogLevel.Error);

            bool found = false;
            foreach (var ns in _namespaces)
            {
                if (!ns.NameSpace.StartsWith("SipSharp.Transports")) continue;
                found = true;
                break;
            }
            if (!found)
                AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        }

        /// <summary>
        /// Add a namespace filter.
        /// </summary>
        /// <param name="ns">Namespace to add filter for.</param>
        /// <param name="level">Minimum loglevel required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType(typeof(SipParser), LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        public void AddNamespace(string ns, LogLevel level)
        {
            lock (_namespaces)
                _namespaces.Add(new NamespaceFilter(ns, level));
        }

        /// <summary>
        /// Add filter for a type
        /// </summary>
        /// <param name="type">Type to add filter for.</param>
        /// <param name="level">Minimum loglevel required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType(typeof(SipParser), LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        public void AddType(Type type, LogLevel level)
        {
            lock (_types)
                _types.Add(type, level);
        }

        /// <summary>
        /// Add filter for a type
        /// </summary>
        /// <param name="typeStr">Type to add filter for.</param>
        /// <param name="level">Minimum loglevel required.</param>
        /// <example>
        /// <code>
        /// // Parsing can only add error and fatal messages
        /// AddNamespace("SipSharp.Messages.Headers.Parsers", LogLevel.Error);
        /// AddType("SipSharp.Messages.MessageFactory", LogLevel.Error);
        /// 
        /// // Transport layer can only log warnings, errors and fatal messages
        /// AddNamespace("SipSharp.Transports.*", LogLevel.Warning);
        /// </code>
        /// </example>
        public void AddType(string typeStr, LogLevel level)
        {
            Type type = Type.GetType(typeStr);
            if (type == null)
                throw new ArgumentException("Type could not be identified.");

            lock (_types)
                _types.Add(type, level);
        }

        #region ILogFilter Members

        /// <summary>
        /// Checks if the specified type can send
        /// log entries at the specified level.
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="type">Type that want to write a log entry.</param>
        /// <returns><c>true</c> if logging is allowed; otherwise <c>false</c>.</returns>
        bool ILogFilter.CanLog(LogLevel level, Type type)
        {
            lock (_types)
            {
                LogLevel allowedLevel;
                if (_types.TryGetValue(type, out allowedLevel))
                    return level >= allowedLevel;
            }

            lock (_namespaces)
            {
                foreach (NamespaceFilter filter in _namespaces)
                {
                    if (filter.IsWildcard)
                        if (type.Namespace.StartsWith(filter.NameSpace))
                            return level >= filter.Level;
                    if (filter.NameSpace.Equals(type.Namespace))
                        return level >= filter.Level;
                }
            }

            return true;
        }

        #endregion

        #region Nested type: NamespaceFilter

        private class NamespaceFilter
        {
            public NamespaceFilter(string ns, LogLevel level)
            {
                if (ns == "*" || ns == ".*")
                    throw new ArgumentException(
                        "No filters = everything logged. NullLogFactory = no logs. Don't use a rule with '*' or '.*'");

                NameSpace = ns;
                int pos = NameSpace.IndexOf('*');
                if (pos > 0)
                {
                    NameSpace = NameSpace[pos - 1] == '.' ? NameSpace.Remove(pos - 1) : NameSpace.Remove(pos);
                    IsWildcard = true;
                }
                Level = level;
            }

            /// <summary>
            /// User have specified a wilcard filter.
            /// </summary>
            /// <remarks>
            /// Wildcard filters are used to log a namespace and
            /// all it's children namespaces.
            /// </remarks>
            public bool IsWildcard { get; private set; }

            public LogLevel Level { get; private set; }
            public string NameSpace { get; private set; }
        }

        #endregion
    }
}