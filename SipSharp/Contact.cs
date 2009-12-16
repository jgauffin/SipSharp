using System;

namespace SipSharp
{
    public class Contact : IEquatable<Contact>, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public Contact(IKeyValueCollection parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="uri">The URI.</param>
        public Contact(string name, SipUri uri)
        {
            Name = name;
            Uri = uri;
            Parameters = new KeyValueCollection();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> class.
        /// </summary>
        /// <param name="value">contact to deep clone.</param>
        public Contact(Contact value)
        {
            Name = value.Name;
            Uri = value.Uri;
            Parameters = new KeyValueCollection();
            foreach (var parameter in value.Parameters)
                Parameters.Add(parameter.Key, parameter.Value);
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
        /// <para>
        /// Should not get confused with the URI parameters.
        /// </para>
        /// <para>
        /// All parameters are automatically converted to lower case.
        /// </para>
        /// </remarks>
        public IKeyValueCollection Parameters { get; private set; }

        /// <summary>
        /// Gets the quality.
        /// </summary>
        /// <value>The quality.</value>
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
            return Parameters.Contains(name.ToLower());
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

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
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

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new Contact(this);
        }
    }
}
