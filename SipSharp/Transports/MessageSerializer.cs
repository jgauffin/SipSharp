using System.IO;
using System.Text;

namespace SipSharp.Transports
{
    /// <summary>
    /// Used to serialize messages into streams.
    /// </summary>
    public class MessageSerializer
    {
        public virtual int Serialize(IRequest request, byte[] buffer)
        {
            var stream = new MemoryStream(buffer);
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(request.Method);
            writer.Write(" ");
            writer.Write(request.Uri.ToString());
            writer.Write(" ");
            writer.WriteLine(request.SipVersion);

            WriteHeader(writer, "To", request.To);
            WriteHeader(writer, "From", request.From);
            WriteHeader(writer, "Contact", request.Contact);
            WriteHeader(writer, "Via", request.Via);
            WriteHeader(writer, "Max-Forwards", request.MaxForwards);
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
                var ms = (MemoryStream) request.Body;
                ms.WriteTo(stream);
            }

            writer.Flush();

            string temp = Encoding.ASCII.GetString(buffer, 0, (int) stream.Length);
            return (int) stream.Position;
        }

        public virtual int Serialize(IResponse response, byte[] buffer)
        {
            var stream = new MemoryStream(buffer);
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write(response.SipVersion);
            writer.Write(" ");
            writer.Write((int) response.StatusCode);
            writer.Write(" ");
            writer.WriteLine(response.ReasonPhrase);

            WriteHeader(writer, "To", response.To);
            WriteHeader(writer, "From", response.From);
            WriteHeader(writer, "Contact", response.Contact);
            WriteHeader(writer, "Via", response.Via);
            WriteHeader(writer, "CSeq", response.CSeq);
            //WriteHeader(writer, "Call-Id", response.CallId);
            foreach (var header in response.Headers)
                WriteHeader(writer, header.Key, header.Value.ToString());
            if (response.Body != null && response.Body.Length > 0)
                WriteHeader(writer, "Content-Length", response.Body.Length);
            else
                WriteHeader(writer, "Content-Length", "0");
            writer.WriteLine();

            if (response.Body != null)
            {
                var ms = (MemoryStream) response.Body;
                ms.WriteTo(stream);
            }

            writer.Flush();

            string temp = Encoding.ASCII.GetString(buffer, 0, (int) stream.Length);
            return (int) stream.Position;
        }

        protected void WriteHeader(TextWriter writer, string name, object value)
        {
            if (value == null)
                return;
            writer.Write(name);
            writer.Write(": ");
            writer.WriteLine(value.ToString());
        }
    }
}