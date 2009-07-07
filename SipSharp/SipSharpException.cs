using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp
{
    public class SipSharpException : Exception
    {
    	public SipSharpException(string errMsg) : base(errMsg)
    	{
    		
    	}
    }
}
