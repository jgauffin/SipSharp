namespace SipSharp.Dialogs
{
    /// <summary>
    /// A dialog represents a peer-to-peer SIP relationship between two user agents
    /// that persists for some time. The dialog facilitates sequencing of messages
    /// between the user agents and proper routing of requests between both of them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A dialog is identified at each UA with a dialog ID, which consists of a Call-ID
    /// value, a local tag and a remote tag. The dialog ID at each UA involved in the
    /// dialog is not the same. Specifically, the local tag at one UA is identical to
    /// the remote tag at the peer UA. The tags are opaque tokens that facilitate the
    /// generation of unique dialog IDs
    /// </para>
    /// <para>
    /// A dialog ID is also associated with all responses and with any request that
    /// contains a tag in the To field. The rules for computing the dialog ID of a
    /// message depend on whether the SIP element is a UAC or UAS. For a UAC, the
    /// Call-ID value of the dialog ID is set to the Call-ID of the message, the remote
    /// tag is set to the tag in the To field of the message, and the local tag is set
    /// to the tag in the From field of the message (these rules apply to both requests
    /// and responses). As one would expect for a UAS, the Call-ID value of the dialog
    /// ID is set to the Call-ID of the message, the remote tag is set to the tag in
    /// the From field of the message, and the local tag is set to the tag in the To
    /// field of the message.
    /// </para>
    /// 
    /// </remarks>
    public interface IDialog
    {
        /// <summary>
        /// Gets or sets unique identifier for the call leg.
        /// </summary>
        string CallId { get; set; }

        /// <summary>
        /// Gets or sets local tag.
        /// </summary>
        /// 
        string LocalTag { get; set; }

        /// <summary>
        /// Gets or sets remote tag.
        /// </summary>
        string RemoteTag { get; set; }

        /// <summary>
        /// Gets or sets dialog state.
        /// </summary>
        DialogState State { get; set; }
    }
}