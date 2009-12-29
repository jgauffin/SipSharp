using System;
using System.Collections.Generic;

namespace SipSharp.Messages.Headers
{
    /// <summary>
    /// Sip contact
    /// </summary>
    public class ContactHeader : IHeader
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactHeader"/> class.
        /// </summary>
        /// <param name="name">Name of the header.</param>
        public ContactHeader(string name)
        {
            _name = name;
            Contacts = new List<Contact>();
        }

        /// <summary>
        /// Gets a list of all specified contacts.
        /// </summary>
        public List<Contact> Contacts { get; internal set; }

        /// <summary>
        /// Gets first contact in list.
        /// </summary>
        public Contact FirstContact { get { return Contacts[0]; } }

        /// <summary>
        /// Gets a list of all parameters for the first contact.
        /// </summary>
        public IKeyValueCollection Parameters
        {
            get { return FirstContact.Parameters; }
        }

        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return _name; }
        }

        #endregion

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public const string NAME = "Contact";
        public const string LNAME = "contact";

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
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string temp = string.Empty;
            foreach (var contact in Contacts)
                temp += contact + ", ";

            return temp.Remove(temp.Length - 2);
        }
    }
}