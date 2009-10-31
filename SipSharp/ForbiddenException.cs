using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp
{
    /// <summary>
    /// Too many authentication attempts, or user may not access specified resource.
    /// </summary>
    public class ForbiddenException : SipException
    {
        public ForbiddenException(string errMsg) : base(SipSharp.StatusCode.Forbidden, errMsg)
        {}
    }
}
