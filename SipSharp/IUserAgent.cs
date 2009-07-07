using System.Net;

namespace SipSharp
{
    /// <summary>
    /// A connected entity (probably a phone).
    /// </summary>
    public interface IUserAgent
    {
        /// <summary>
        /// Gets remote end point.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets or sets user agent name.
        /// </summary>
        string Name { get; set; }
    }
}
