using System;
using System.Collections.Specialized;
using SipSharp.Tools;
using Xunit;

namespace SipSharp.Headers
{
    /// <summary>
    /// Sip contact
    /// </summary>
    public class Contact : IHeader
    {
        private readonly string _name;
        private NameValueCollection _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        public Contact(string name)
        {
            _name = name;
            _parameters = new NameValueCollection();
        }

        /// <summary>
        /// Gets or sets name of contact.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets URI to use.
        /// </summary>
        public SipUri Uri { get; set; }

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets header value(s).
        /// </summary>
        /// <remarks>
        /// Values will be combined (separated by comma) if more than
        /// one value exists.
        /// </remarks>
        string IHeader.Value
        {
            get
            {
                if (DisplayName == null)
                    return "<" + Uri + ">";
                return "\"" + DisplayName + "\" <" + Uri + ">";
            }
        }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] IHeader.Values
        {
            get { return new[] {((IHeader) this).Value}; }
        }

        public NameValueCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times.
        /// </remarks>
        /// <param name="value">string containing one or more values.</param>
        /// <exception cref="FormatException">If value format is incorrect.</exception>
        void IHeader.Parse(string value)
        {
            StringReader reader = new StringReader(value);
            reader.SkipWhiteSpaces();

            // no name
            if (reader.Current != '<')
            {
                DisplayName = reader.ReadWord();
                reader.SkipWhiteSpaces();
            }

            if (reader.Current != '<')
                throw new FormatException("Expected a contact after the display name.");

            reader.MoveNext();
            string uri = reader.Read('>');
            if (uri == null)
                throw new FormatException("Missing '>' to indicate end of contact.");

            Uri = SipUri.Parse(uri);
            if (!reader.MoveNext())
                return;

            reader.ParseParameters(_parameters);
        }

        [Fact]
        private static void Test()
        {
            Contact contact = new Contact("To");
            ((IHeader)contact).Parse(@"""Jonas Gauffin"" <sip:jonas@gauffin.com>");
            Assert.Equal("Jonas Gauffin", contact.DisplayName);
            Assert.Equal("sip:jonas@gauffin.com", contact.Uri.ToString());

            ((IHeader)contact).Parse(@"""Jonas \""Gauffin\"""" <sip:jonas@gauffin.com>");
            Assert.Equal("Jonas \\\"Gauffin\\\"", contact.DisplayName);

            ((IHeader)contact).Parse(@"""Jonas Gauffin"" <sip:jonas@gauffin.com>;lr=true;abc=false");

        }
    }
}
