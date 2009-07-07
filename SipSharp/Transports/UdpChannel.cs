/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SipSharp.Transports
{
    /// <summary>
    /// Sends messages over UDP.
    /// </summary>
    public class UdpChannel : IChannel
    {
        private readonly bool _canRead;
        private MessageSerializer _serializer;
        private Socket _socket;
        private IPEndPoint _remoteEndPoint;
        private MemoryStream _stream;
        private bool _isSending;
        private Queue<byte[]> _sendQueue = new Queue<byte[]>();

        ///<summary>
        ///
        ///</summary>
        ///<param name="canRead">We can read stuff from the socket.</param>
        public UdpChannel(bool canRead)
        {
            _canRead = canRead;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _stream = new MemoryStream();
        }


        /// <summary>
        /// Sends a byte buffer to remove side.
        /// </summary>
        /// <param name="buffer"></param>
        /// <remarks>
        /// buffers should be placed in a queue and processed asynchronously.
        /// </remarks>
        private void Send(byte[] buffer)
        {
            int bytesSent = _socket.SendTo(buffer, _remoteEndPoint);
            while (bytesSent < buffer.Length)
            {
                bytesSent += _socket.SendTo(buffer, bytesSent, buffer.Length - bytesSent, SocketFlags.None,
                                            _remoteEndPoint);
            }
        }



        /// <summary>
        /// Send a message to an endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">End point that should receive the message.</param>
        /// <param name="buffer">Buffer containing bytes to send.</param>
        public void Send(IPEndPoint remoteEndPoint, byte[] buffer)
        {
            lock (_sendQueue)
            {
                if (!_isSending)
                {
                    _socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEndPoint, OnSendComplete, null);
                    _isSending = true;
                }

                _sendQueue.Enqueue(buffer);
            }
        }

        private void OnSendComplete(IAsyncResult ar)
        {
            lock (_sendQueue)
            {
                if (_sendQueue.Count == 0)
                {
                    _isSending = false;
                    return;
                }

                byte[] buffer = _sendQueue.Dequeue();
                _socket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, _remoteEndPoint, OnSendComplete, null);
            }
        }

        /// <summary>
        /// Start channel, to be able to process incoming bytes.
        /// </summary>
        public void Start()
        {
            
        }

        /// <summary>
        /// Close channel.
        /// </summary>
        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public event EventHandler<RequestEventArgs> RequestReceived;
        public event EventHandler<ResponseEventArgs> ResponseReceived;
    }
}
*/