/*
using System;
using System.Net;
using System.Net.Sockets;
using SipSharp.Transports.Parser;

namespace SipSharp.Transports
{
    /// <summary>
    /// TCP channel
    /// </summary>
    public class TcpChannel : Channel, IChannel
    {
        private Socket _socket;
        private NetworkStream _stream;
        private byte[] _buffer = new byte[16834];
        private ILogWriter _log = LogFactory.CreateLogger(typeof (TcpChannel));

        public TcpChannel(Socket socket, IMessageParser parser)
            : base(parser)
        {
            _socket = socket;
            _stream = new NetworkStream(_socket);
        }

        public void Start()
        {
            _stream.BeginRead(_buffer, 0, _buffer.Length, OnRead, null);
        }

        /// <summary>
        /// Close channel.
        /// </summary>
        public void Close()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(true);
            _stream.Close();
            _stream = null;
        }



        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int bytesRead = _stream.EndRead(ar);
                if (bytesRead == 0)
                {
                    Disconnect(SocketError.ConnectionReset);
                    return;
                }

                int offset = Parse(_buffer, 0, bytesRead);
                _stream.BeginRead(_buffer, offset, _buffer.Length - offset, OnRead, null);
            }
            catch(SocketException err)
            {
                _log.Write(this, LogLevel.Debug, _socket.RemoteEndPoint + " got disconnected: " + err);
                Disconnect(err.SocketErrorCode);
            }
        }

        protected void Disconnect(SocketError error)
        {
            Close();
        }

        public void Send(IRequest request)
        {

            _stream.BeginWrite()
        }

        public void Send(IResponse response)
        {
            
        }

        /// <summary>
        /// A request have been successfully parsed.
        /// </summary>
        /// <param name="request">Request</param>
        protected override void OnRequestCompleted(IRequest request)
        {
            RequestReceived(this, new RequestEventArgs(request, (IPEndPoint)_socket.RemoteEndPoint));
        }

        /// <summary>
        /// A response have been successfully parsed.
        /// </summary>
        /// <param name="response">the response</param>
        protected override void OnResponseCompleted(IResponse response)
        {
            ResponseReceived(this, new ResponseEventArgs(response, (IPEndPoint)_socket.RemoteEndPoint));
        }
    }
}
*/