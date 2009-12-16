using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Calls
{
    public class PhoneNumber
    {
        /// <summary>
        /// Gets or sets number without area and country codes.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets area code.
        /// </summary>
        public string AreaCode { get; set; }

        /// <summary>
        /// Gets or sets code.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets number formatted as E.164.
        /// </summary>
        public string E164 { get; set; }


    }
}
