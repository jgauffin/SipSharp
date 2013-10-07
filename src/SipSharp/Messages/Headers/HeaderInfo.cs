using SipSharp.Messages.Headers.Parsers;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Contains header name
    /// </summary>
    public class HeaderInfo
    {
        /// <summary>
        /// Gets or sets header name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets header parser.
        /// </summary>
        public IHeaderParser Parser { get; set; }

        /// <summary>
        /// Gets or sets compact name
        /// </summary>
        public char ShortName { get; set; }
    }
}