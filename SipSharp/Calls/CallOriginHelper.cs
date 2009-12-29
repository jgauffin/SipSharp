namespace SipSharp.Calls
{
    public static class CallOriginHelper
    {
        public static bool IsExternalInbound(this CallOrigins origins)
        {
            return true;
        }

        public static bool IsInternalInbound(this CallOrigins origins)
        {
            return true;
        }
    }
}