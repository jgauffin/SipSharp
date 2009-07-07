namespace SipSharp
{
	/// <summary>
	/// Request syntax was incorrect.
	/// </summary>
    public class BadRequestException : SipException
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="BadRequestException"/> class.
		/// </summary>
		/// <param name="msg">Why exception was thrown.</param>
        public BadRequestException(string msg) : base(StatusCode.BadRequest, msg)
        {
            
        }
    }
}
