using System.IO;

namespace SipSharp.Transports
{
    /// <summary>
    /// Used to serialize messages into streams.
    /// </summary>
    public class MessageSerializer
    {
        public virtual void Serialize(IRequest request, Stream stream)
        {
            
        }

        public virtual void Serialize(IResponse response, Stream stream)
        {
            
        }
    }
}
