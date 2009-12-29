using System;

namespace SipSharp.Messages.Headers
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
    /// <example>
    /// 1201 INVITE
    /// </example>
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
            Number = seqNr;
            Method = method;
        }

        private string _method;

        ///<summary>
        ///Gets or sets SIP method.
        ///</summary>
        /// <remarks>
        /// Will always be converted into upper case.
        /// </remarks>
        public string Method
        {
            get { return _method; }
            set { _method = value == null ? null : value.ToUpper(); }
        }

        /// <summary>
        /// Gets or sets sequence number.
        /// </summary>
        public int Number { get; set; }

        #region IHeader Members

        /// <summary>
        /// Gets or sets name of header.
        /// </summary>
        string IHeader.Name
        {
            get { return "CSeq"; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new CSeq(Number, Method);
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(IHeader other)
        {
            CSeq cs = other as CSeq;
            if (cs == null)
                return false;

            return cs.Method.Equals(Method, StringComparison.OrdinalIgnoreCase) && cs.Number == Number;
        }

        public override bool Equals(object obj)
        {
            CSeq cs = obj as CSeq;
            if (cs == null)
                return false;

            return cs.Method.Equals(Method, StringComparison.OrdinalIgnoreCase) && cs.Number == Number;
        }

        public override string ToString()
        {
            return Number + " " + Method;
        }
    }
}