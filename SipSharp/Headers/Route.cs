namespace SipSharp.Headers
{
    /// <summary>
    /// Route or Record-Route header.
    /// </summary>
    public class Route : MultiHeader<RouteEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHeader&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">Header name.</param>
        public Route(string name) : base(name)
        {
        }

        /// <summary>
        /// Create a new header value instance.
        /// </summary>
        /// <returns>A newly created header.</returns>
        protected override IHeader CreateHeader()
        {
            return new RouteEntry(((IHeader)this).Name);
        }
    }
}
