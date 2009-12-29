using System;

namespace SipSharp.Messages.Headers
{
    public class ContentType : IHeader
    {
        /// <summary>
        /// Gets header name
        /// </summary>
        public const string NAME = "Content-Type";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentType"/> class.
        /// </summary>
        public ContentType()
        {
            Parameters = new KeyValueCollection();
        }

        private ContentType(ContentType value)
        {
            Type = value.Type;
            SubType = value.SubType;
            foreach (var parameter in value.Parameters)
                Parameters.Add(parameter.Key, parameter.Value);
        }

        /// <summary>
        /// Gets any parameters
        /// </summary>
        public IKeyValueCollection Parameters { get; private set; }

        /// <summary>
        /// Gets or sets sub type.
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// Gets or sets content type.
        /// </summary>
        public string Type { get; set; }

        #region IHeader Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new ContentType(this);
        }

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return NAME; }
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(IHeader other)
        {
            ContentType ct = other as ContentType;
            if (ct == null)
                return false;
            return string.Compare(ct.Type, Type, true) == 0 && string.Compare(ct.SubType, SubType, true) == 0;
        }
    }
}