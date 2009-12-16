using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SipSharp.Messages.Headers;

namespace SipSharp.Dialogs
{
    public class Dialog : IDialog
    {
        /// <summary>
        /// Gets or sets unique identifier for the call leg.
        /// </summary>
        public string CallId { get; set; }

        /// <summary>
        /// Gets or sets remote tag.
        /// </summary>
        public string RemoteTag { get; set; }

        /// <summary>
        /// Gets or sets local tag.
        /// </summary>
        /// 
        public string LocalTag { get; set; }

        /// <summary>
        /// Gets or sets dialog state.
        /// </summary>
        public DialogState State { get; set; }

        public string IsSecure
        {
            get; set;
        }

        Via RouteSet { get; set; }

        public Route Route { get; set; }

        public ContactHeader RemoteTarget { get; set; }
    }
}
