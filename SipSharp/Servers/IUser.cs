using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Servers
{
    public interface IUser
    {
        IList<Contact> Contacts { get; }

        string DisplayName { get; }

        DateTime ExpiresAt { get; }

        int Expires { get; set; }
    }
}
