using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Calls
{
    public static class CallOriginHelper
    {
        public static bool IsInternalInbound(this CallOrigins origins)
        {
            return true;
        }

        public static bool IsExternalInbound(this CallOrigins origins)
        {
            return true;
        }
    }
}
