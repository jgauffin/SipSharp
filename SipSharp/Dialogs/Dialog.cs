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


        /// <summary>
        /// Gets or sets local sequence number.
        /// </summary>
        public int LocalSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets remote sequence number.
        /// </summary>
        public int RemoteSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets remote URI
        /// </summary>
        public SipUri RemoteUri { get; set; }

        /// <summary>
        /// Gets or sets local URI.
        /// </summary>
        public SipUri LocalUri { get; set; }

        /// <summary>
        /// Gets or sets if dialog is sent over a secure transport.
        /// </summary>
        public bool IsSecure
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets route
        /// </summary>
        /// <remarks>
        /// The route set MUST be set to the list of URIs in the Record-Route
        /// header field from the response, taken in reverse order and preserving
        /// all URI parameters.  If no Record-Route header field is present in
        /// the response, the route set MUST be set to the empty set.  This route
        /// set, even if empty, overrides any pre-existing route set for future
        /// requests in this dialog.  
        /// </remarks>
        public Route RouteSet { get; set; }

        /// <summary>
        /// Gets or sets remote target.
        /// </summary>
        /// <remarks>
        /// </para>
        /// This is the value of
        /// the Contact header of received Responses for target refresh Requests in
        /// this dialog when acting as an User Agent Client.
        /// </para><para>
        /// This is the value of the Contact header of received target refresh
        /// Requests Requests in this dialog when acting as an User Agent Server.
        /// </para>
        /// </remarks>
        /// 
        public ContactHeader RemoteTarget { get; set; }
    }
}
