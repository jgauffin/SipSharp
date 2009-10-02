using System.IO;
using System.Text;
using SipSharp.Tools;

namespace SipSharp.Transports
{
    /// <summary>
    /// Used to serialize messages into streams.
    /// </summary>
    public class MessageSerializer
    {
        public virtual int Serialize(IRequest request, byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(request.Method);
            writer.Write(" ");
            writer.Write(request.Uri.ToString());
            writer.Write(" ");
            writer.WriteLine(request.SipVersion);

            WriteHeader(writer, "To", request.To);
            WriteHeader(writer, "From", request.From);
            WriteHeader(writer, "Contact", request.Contact);
            WriteHeader(writer, "Via", request.Via);
            WriteHeader(writer, "MaxForwards", request.MaxForwards);
            WriteHeader(writer, "CSeq", request.CSeq);
            WriteHeader(writer, "Call-ID", request.CallId);
            foreach (var header in request.Headers)
                WriteHeader(writer, header.Key, header.Value.ToString());
            if (request.Body != null && request.Body.Length > 0)
                WriteHeader(writer, "Content-Length", request.Body.Length);
            else
                WriteHeader(writer, "Content-Length", "0");
            writer.WriteLine();

            if (request.Body != null)
            {
                MemoryStream ms = (MemoryStream)request.Body;
                ms.WriteTo(stream);
            }

            writer.Flush();

            string temp = Encoding.ASCII.GetString(buffer, 0, (int)stream.Length);
            return (int)stream.Position;
        }

        protected void WriteHeader(TextWriter writer, string name, object value)
        {
            if (value == null)
                return;
            writer.Write(name);
            writer.Write(": ");
            writer.WriteLine(value.ToString());
        }

        public virtual int Serialize(IResponse response, byte[] buffer)
        {
            return 0;
        }
    }
}
