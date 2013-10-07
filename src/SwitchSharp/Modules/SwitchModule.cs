using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp;

namespace SwitchSharp.Modules
{
    public interface ISwitchModule
    {
        /// <summary>
        /// A new request have come.
        /// </summary>
        /// <param name="context">Request context</param>
        /// <returns>What we should to with the request.</returns>
        ProcessingResult ProcessRequest(RequestContext context);
    }
}
