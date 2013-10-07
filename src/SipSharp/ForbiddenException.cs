namespace SipSharp
{
    /// <summary>
    /// Too many authentication attempts, or user may not access specified resource.
    /// </summary>
    public class ForbiddenException : SipException
    {
        public ForbiddenException(string errMsg) : base(StatusCode.Forbidden, errMsg)
        {
        }
    }
}