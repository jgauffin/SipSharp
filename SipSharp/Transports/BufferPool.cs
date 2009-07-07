using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Transports
{
    class BufferPool
    {
        private Queue<byte[]> _buffers = new Queue<byte[]>();
        private int _size;

        public BufferPool(int bufferSize)
        {
            _size = bufferSize;
        }

        public byte[] Dequeue()
        {
            lock (_buffers)
            {
                if (_buffers.Count == 0)
                    return new byte[_size];
                return _buffers.Dequeue();
            }
        }

        public void Enqueue(byte[] buffer)
        {
            lock (_buffers)
                _buffers.Enqueue(buffer);
        }
    }
}
