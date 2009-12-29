namespace SipSharp.Client
{
    internal class SipClient
    {
        private readonly ISipStack _stack;
        private Contact _contact;

        public SipClient(ISipStack stack)
        {
            _stack = stack;
        }

        public void MakeCall(Contact to)
        {
        }
    }
}