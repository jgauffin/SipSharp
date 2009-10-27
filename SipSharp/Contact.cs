using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SipSharp
{
    public class Contact : IEquatable<Contact>
    {
        public Contact(IKeyValueCollection parameters)
        {
            Parameters = parameters;
        }

        public Contact(string name, SipUri uri)
        {
            Name = name;
            Uri = uri;
            Parameters = new KeyValueCollection();
        }

        /// <summary>
        /// Gets or sets contact's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets where contact can be reached
        /// </summary>
        public SipUri Uri { get; set; }

        /// <summary>
        /// Gets all contact parameters.
        /// </summary>
        /// <remarks>
        /// Should not get confused with the URI parameters.
        /// </remarks>
        public IKeyValueCollection Parameters { get; private set; }

        public double Quality
        {
            get
            {
                double temp;
                return !double.TryParse(Parameters["q"], out temp) ? 1 : temp;
            }
        }


        /// <summary>
        /// Checks if a parameter exists.
        /// </summary>
        /// <param name="name">Name of parameter</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool HasParameter(string name)
        {
            return Parameters.Contains(name);
        }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(Contact other)
        {
            return other.Uri.Equals(Uri) && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            string value;
            if (!string.IsNullOrEmpty(Name))
                value = "\"" + Name + "\" ";
            else
                value = string.Empty;

            value += "<" + Uri + ">";

            foreach (var parameter in Parameters)
            {
                value += ";" + parameter.Key;
                if (!string.IsNullOrEmpty(parameter.Value))
                    value += "=" + parameter.Value;
            }

            return value;
        }

        public override bool Equals(object obj)
        {
            Contact other = obj as Contact;
            if (other == null)
                return false;

            return other.Uri == Uri && Name == other.Name;
        }
    }
}
