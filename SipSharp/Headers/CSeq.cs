using System;

namespace SipSharp.Headers
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    /// <remarks>
    /// <para>
    /// The CSeq header field serves as a way to identify and order
    /// transactions.  It consists of a sequence number and a method.  The
    /// method MUST match that of the request.  For non-REGISTER requests
    /// outside of a dialog, the sequence number value is arbitrary.  The
    /// sequence number value MUST be expressible as a 32-bit unsigned
    /// integer and MUST be less than 2**31.  As long as it follows the above
    /// guidelines, a client may use any mechanism it would like to select
    /// CSeq header field values.
    /// </para>
    /// </remarks>
    public sealed class CSeq : IHeader
    {
        /// <summary>
        /// Unspecified sequence number.
        /// </summary>
        public static readonly CSeq Empty = new CSeq(0, string.Empty);


        /// <summary>
        /// Initializes a new instance of the <see cref="CSeq"/> class.
        /// </summary>
        public CSeq()
        {
            Method = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSeq"/> class.
        /// </summary>
        /// <param name="seqNr">Sequence number.</param>
        /// <param name="method">SIP method.</param>
        public CSeq(int seqNr, string method)
        {
            SeqNr = seqNr;
            Method = method;
        }

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name { get { return "CSeq"; } }

        /// <summary>
        /// Value
        /// </summary>
        /// <example>
        /// application/sdp
        /// </example>
        /// <exception cref="ArgumentException"></exception>
        public string Value
        {
            get { return string.Format("{0} {1}", SeqNr, Method); }
        }

        /// <summary>
        /// Gets all header values.
        /// </summary>
        string[] IHeader.Values
        {
            get { return new [] {Value}; }
        }

        /// <summary>
        /// Parse a value.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times.
        /// </remarks>
        /// <param name="value">string containing one or more values.</param>
        /// <exception cref="FormatException">If value format is incorrect.</exception>
        void IHeader.Parse(string value)
        {
            int pos = value.IndexOf(' ');
            if (pos == -1)
                throw new FormatException("Invalid CSeq: " + value);

            int seqNr;
            if (!int.TryParse(value.Substring(0, pos), out seqNr))
                throw new FormatException("Invalid CSeq: " + value);
            SeqNr = seqNr;

            Method = value.Substring(pos + 1).Trim();
            if (Method == string.Empty)
                throw new FormatException("Invalid CSeq: " + value);
        }

        /// <summary>
        /// Gets or sets sequence number.
        /// </summary>
        public int SeqNr { get; set; }

        ///<summary>
        ///Gets or sets SIP method.
        ///</summary>
        public string Method { get; set; }
    }
}
