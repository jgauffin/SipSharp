using System;
using System.Collections.Generic;

namespace SipSharp.Servers
{
    public interface IUser
    {
        IList<Contact> Contacts { get; }

        string DisplayName { get; }

        int Expires { get; set; }
        DateTime ExpiresAt { get; }
    }
}