using System;
using System.IO;
using System.Threading;

namespace Remact.Net.TcpStream
{

    /// <summary>
    /// Implements a stream for deserializing a message from a byte buffer.
    /// The buffer is contained e.g. in SocketAsyncEventArgs.
    /// Normally the buffer contains the whole message. Then the message is deserialized without blocking.
    /// The thread that deserializes is normally the threadpool thread that has received the first bytes of the message.
    /// When the buffer only contains a partial message, then the deserializing thread is blocked until the rest arrives.
    /// </summary>
    public class TcpStreamIncoming : Stream, IDisposable
    {
        private byte[] _socketBuffer;
        private int _bufferLength;
        private int _nextByteInBuffer;
        private Func<bool> _startAsyncRead;
        private ManualResetEventSlim _resetEvent;

        /// <summary>
        /// Creates a readable stream using Async IO.
        /// </summary>
        /// <param name="startAsyncRead">A function to start the next asynchronous read operation.</param>
        public TcpStreamIncoming(Func<bool> startAsyncRead)
        {
            if (startAsyncRead == null)
            {
                throw new ArgumentNullException("startAsyncRead");
            }
            _startAsyncRead = startAsyncRead;
            _resetEvent = new ManualResetEventSlim();
        }

        public override bool CanRead {get { return true; }}

        public override bool CanSeek {get { return false; }}

        public override bool CanWrite {get { return false; }}

        /// <summary>
        /// Called by the SocketAsyncEventArgs callback (on a threadpool thread), when data has been added to the SocketAsyncEventArgs buffer.
        /// </summary>
        public void DataReceived(byte[] socketBuffer, int availableBytes, bool userReadsMessage)
        {
            _socketBuffer = socketBuffer;
            _bufferLength = availableBytes;
            _nextByteInBuffer = 0;
            if (userReadsMessage)
            {
                _resetEvent.Set();
            }
        }

        private bool WaitForMoreData()
        {
            _resetEvent.Reset();
            if (!_startAsyncRead())
            {
                return false; // socket closed
            }
            _resetEvent.Wait(); // TODO cancellation/timeout
            return true;
        }

        /// <summary>
        /// Return the next byte on the user thread. May wait asynchronously until new data has arrived.
        /// Returns -1 when stream is closed.
        /// </summary>
        public override int ReadByte()
        {
            if (_nextByteInBuffer >= _bufferLength)
            {
                if (!WaitForMoreData())
                {
                    return -1;
                }
            }

            return _socketBuffer[_nextByteInBuffer++];
        }

        /// <summary>
        /// Returns the count of bytes read on the user thread. May wait asynchronously until new data has arrived.
        /// Returns 0 when the stream is closed.
        /// </summary>
        public override int Read(byte[] userBuffer, int offset, int count)
        {
            int totalCount = 0;
            while (count > 0)
            {
                var rest = _bufferLength - _nextByteInBuffer;
                if (rest >= count)
                {
                    Array.Copy(_socketBuffer, _nextByteInBuffer, userBuffer, offset, count);
                    _nextByteInBuffer += count;
                    return totalCount + count; // all data read successfully
                }

                if (rest > 0)
                {
                    // copy rest of socket buffer to user buffer
                    Array.Copy(_socketBuffer, _nextByteInBuffer, userBuffer, offset, rest); 
                    _nextByteInBuffer += rest;
                    offset += rest;
                    totalCount += rest;
                    count -= rest;
                }

                if (!WaitForMoreData())
                {
                    break; // socket closed
                }
            }
            return totalCount;
        }


        #region Not supported functions

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true; //prevent second call to Dispose
                if (disposing)
                {
                    _resetEvent.Set();
                    _resetEvent.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
