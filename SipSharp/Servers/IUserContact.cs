using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Servers
{
    /// <summary>
    /// A user may have multiple contacts.
    /// </summary>
    public interface IUserContact
    {
        /// <summary>
        /// Gets or sets sequence used in the last registration.
        /// </summary>
        string CSeq { get; set; }

        /// <summary>
        /// Gets or sets E.164 formatted phone number.
        /// </summary>
        string PhoneNumber { get; set; }
    }
}
