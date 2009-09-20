using System;
using SipSharp.Tools;

namespace SipSharp.Transports
{
    /// <summary>
    /// Keeps a pool of byte buffers.
    /// </summary>
    /// <remarks>A simple flyweight pattern to avoid frequent allocation
    /// of small buffers.</remarks>
    public class BufferPool
    {
        private readonly ObjectPool<byte[]> _pool;
        private readonly int _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferPool"/> class.
        /// </summary>
        /// <param name="bufferSize">How large buffers to allocate.</param>
        public BufferPool(int bufferSize)
        {
            _pool = new ObjectPool<byte[]>(CreateBuffer);
            _size = bufferSize;
        }

        /// <summary>
        /// Creates the buffer.
        /// </summary>
        /// <returns></returns>
        private byte[] CreateBuffer()
        {
            return new byte[_size];
        }

        /// <summary>
        /// Get a buffer.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Will create one if queue is empty.</remarks>
        public byte[] Dequeue()
        {
            return _pool.Dequeue();
        }

        /// <summary>
        /// Enqueues the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer to enqueue.</param>
        /// <exception cref="ArgumentOutOfRangeException">Buffer is is less than the minimum requirement.</exception>
        public void Enqueue(byte[] buffer)
        {
            if (buffer.Length < _size)
                throw new InvalidOperationException("Buffer is is less than the minimum requirement: " + buffer.Length);

            _pool.Enqueue(buffer);
        }
    }
}