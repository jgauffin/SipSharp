namespace SipSharp.Headers
{
    /// <summary>
    /// Header containing one or more <see cref="ViaEntry"/> instances.
    /// </summary>
    public class Via : MultiHeader<ViaEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHeader&lt;T&gt;"/> class.
        /// </summary>
        public Via() : base("Via")
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="Via"/> class.
		/// </summary>
		/// <param name="via">Via to deep copy.</param>
    	private Via(Via via) : base(via)
    	{
    	}

    	///<summary>
        ///
        ///</summary>
        ///<returns></returns>
        protected override IHeader CreateHeader()
        {
            return new ViaEntry();
        }

        /// <summary>
        /// Add an entry to the top of the list.
        /// </summary>
        /// <param name="entry"></param>
        public void AddToTop(ViaEntry entry)
        {
            Items.Insert(0, entry);
        }

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
    	public override object Clone()
    	{
    		return new Via(this);
    	}
    }
}
