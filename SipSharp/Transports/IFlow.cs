namespace SipSharp.Transports
{
    /// <summary>
    /// Mapping between a server and a user agent as defined in draft-ietf-sip-outbound-20
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Session Initiation Protocol (SIP) allows proxy servers to
    /// initiate TCP connections or to send asynchronous UDP datagrams to
    /// User Agents in order to deliver requests.  However, in a large number
    /// of real deployments, many practical considerations, such as the
    /// existence of firewalls and Network Address Translators (NATs) or the
    /// use of TLS with server-provided certificates, prevent servers from
    /// connecting to User Agents in this way.  This specification defines
    /// behaviors for User Agents, registrars and proxy servers that allow
    /// requests to be delivered on existing connections established by the
    /// User Agent.  It also defines keep alive behaviors needed to keep NAT
    /// bindings open and specifies the usage of multiple connections from
    /// the User Agent to its Registrar.
    /// </para> <para>
    /// Each UA has a unique instance-id that stays the same for this UA even
    /// if the UA reboots or is power cycled.  Each UA can register multiple
    /// times over different flows for the same SIP Address of Record (AOR)
    /// to achieve high reliability.  Each registration includes the
    /// instance-id for the UA and a reg-id label that is different for each
    /// flow.  The registrar can use the instance-id to recognize that two
    /// different registrations both correspond to the same UA.  The
    /// registrar can use the reg-id label to recognize whether a UA is
    /// creating a new flow or refreshing or replacing an old one, possibly
    /// after a reboot or a network failure.
    /// </para> <para>
    /// When a proxy goes to route a message to a UA for which it has a
    /// binding, it can use any one of the flows on which a successful
    /// registration has been completed.  A failure to deliver a request on a
    /// particular flow can be tried again on an alternate flow.  Proxies can
    /// determine which flows go to the same UA by comparing the instance-id.
    /// Proxies can tell that a flow replaces a previously abandoned flow by
    /// looking at the reg-id.
    /// </para> <para>
    /// When sending a dialog-forming request, a UA can also ask its first
    /// edge proxy to route subsequent requests in that dialog over the same
    /// flow.  This is necessary whether the UA has registered or not.
    /// </para> <para>
    /// UAs use a simple periodic message as a keep alive mechanism to keep
    /// their flow to the proxy or registrar alive.  For connection oriented
    /// transports such as TCP this is based on carriage-return and line-feed
    /// sequences (CRLF), while for transports that are not connection
    /// oriented this is accomplished by using a SIP-specific usage profile
    /// of STUN (Session Traversal Utilities for NAT) [RFC5389].
    /// </para>
    /// </remarks>
    public interface IFlow
    {

        /// <summary>
        /// Gets instance id
        /// </summary>
        /// <remarks>
        /// Instance-id should be a UUID URN [RFC4122].
        /// </remarks>
        string InstanceId { get; }

    }
}