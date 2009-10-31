using SipSharp.Messages.Headers;

namespace SipSharp
{
    /// <summary>
    /// SIP Response.
    /// </summary>
    public interface IResponse : IMessage
    {
        /// <summary>
        /// Gets or sets SIP status code.
        /// </summary>
        StatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets text describing why the status code was used.
        /// </summary>
        string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets where we want to be contacted
        /// </summary>
        ContactHeader Contact { get; set; }

        /// <summary>
        /// Gets or sets all routes.
        /// </summary>
        Route Route { get; }


    }
}
