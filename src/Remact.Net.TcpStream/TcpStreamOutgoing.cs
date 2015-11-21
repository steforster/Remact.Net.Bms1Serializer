using System;
using System.IO;
using System.Threading.Tasks;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// Implements a stream for asynchronously serializing a message to a socket.
    /// The buffer is expandable up to the size of the largest message. The message is serialized without blocking.
    /// The thread that serializes is normally the actor worker thread.
    /// When calling FlushAsync, the message is sent to underlying socket buffer. The task continues when the socket is ready for the next message.
    /// </summary>
    public class TcpStreamOutgoing : MemoryStream, IDisposable
    {
        private Func<byte[], int, Task> _sendAsync;

        /// <summary>
        /// Creates a writable stream using Async IO. Sending to the socket must be implemented by the caller.
        /// </summary>
        public TcpStreamOutgoing(Func<byte[], int, Task> sendAsync, int initialBufferSize)
            : base(initialBufferSize)
        {
            if (sendAsync == null)
            {
                throw new ArgumentNullException("sendAsync");
            }
            _sendAsync = sendAsync;
        }

        public override bool CanRead {get { return false; }}

        public override bool CanSeek {get { return false; }}

        public override bool CanWrite {get { return true; }}

        /// <summary>
        /// After a message has been serialized into the underlying memory stream, FlushAsync must be called to copy it to the TCP socket buffer.
        /// In .NET 4.0 this method is not available in the Stream base class. Therefore, we implement it as virtual.
        /// </summary>
        public virtual Task FlushAsync()
        {
            int length = (int)Position;
            Position = 0;
            return _sendAsync(GetBuffer(), length);
        }
    }
}
