using System.IO;

namespace SipSharp.SDP
{
    /// <summary>
    /// SDP is intended for describing multimedia sessions for the purposes of session
    /// announcement, session invitation, and other forms of multimedia session
    /// initiation.
    /// </summary>
    public class SessionDescriptionProtocol
    {
        public void Parse(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var reader = new StringReader(streamReader.ReadToEnd());
        }
    }
}