using System;
using System.Collections.Generic;
using System.Reflection;
using SipSharp.Logging;
using SipSharp.Messages.Headers.Parsers;
using SipSharp.Tools;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Used to create headers
    /// </summary>
    internal class HeaderFactory
    {
        private readonly Dictionary<string, IHeaderParser> _headerParsers = new Dictionary<string, IHeaderParser>();
        private readonly ILogger _logger = LogFactory.CreateLogger(typeof (MessageFactory));
        private readonly ObjectPool<StringReader> _readers = new ObjectPool<StringReader>(() => new StringReader());
        private readonly Dictionary<char, IHeaderParser> _shortNameParsers = new Dictionary<char, IHeaderParser>();
        public void AddDefaultParsers()
        {
            string ns = GetType().Namespace + ".Parsers";
            Type parserInterface = typeof (IHeaderParser);

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace != ns)
                    continue;
                if (!parserInterface.IsAssignableFrom(type))
                    continue;
                if (type.IsInterface || type.IsAbstract)
                    continue;


                IHeaderParser parser = (IHeaderParser)Activator.CreateInstance(type);
                foreach (var attr in type.GetCustomAttributes(false))
                {
                    // Generic parser should not be added automatically.
                    if (attr is GenericParserAttribute)
                        break;

                    ParserForAttribute attribute = attr as ParserForAttribute;
                    if (attribute != null)
                        AddParser(parser, attribute.Name, attribute.CompactName);
                }
            }
        }

        /// <summary>
        /// Add a header parser.
        /// </summary>
        /// <param name="parser">Parser to use</param>
        /// <param name="name">Header name</param>
        /// <param name="shortName">Compact header name; or <see cref="char.MinValue"/> if not specified.</param>
        public void AddParser(IHeaderParser parser, string name, char shortName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name may not be null or empty.", "name");

            name = name.ToLower();
            shortName = char.ToLower(shortName);

            lock (_headerParsers)
            {
                if (_headerParsers.ContainsKey(name))
                    _logger.Info("Changing parser '" + name + "' from " +
                                 _headerParsers[name].GetType().FullName + " to " + parser.GetType().FullName);
                _headerParsers[name] = parser;
            }

            lock (_shortNameParsers)
                _shortNameParsers[shortName] = parser;
        }

        public IHeaderParser GetParser(string name)
        {
            IHeaderParser header;
            name = name.ToLower();

            // Lookup using standard name.
            if (name.Length == 1)
                _shortNameParsers.TryGetValue(name[0], out header);
            else
                _headerParsers.TryGetValue(name, out header);

            // Use our generic header if it didnt exist in our list.
            if (header == null)
            {
                _logger.Warning("Did not find a parser for header '" + name + "'.");
                return null;
            }

            return header;
        }

        public IHeader Parse(string name, string value)
        {
            _logger.Trace("Parsing [" + name + "] " + value);
            IHeaderParser parser = GetParser(name);
            StringReader reader = _readers.Dequeue();
            try
            {
                if (parser == null)
                    return CreateDefaultHeader(name, value);

                reader.Assign(value);
                return parser.Parse(name, reader);
            }
            finally
            {
                _readers.Enqueue(reader);
            }
        }

        protected virtual IHeader CreateDefaultHeader(string name, string value)
        {
            return new StringHeader(name) {Value = value};
        }
    }
}