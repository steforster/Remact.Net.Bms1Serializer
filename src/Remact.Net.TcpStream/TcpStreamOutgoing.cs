using System;
using System.IO;
using System.Threading.Tasks;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// Implements a stream for asynchronously serializing a message to a socket.
    /// The buffer expands up to the size of the largest message. The message is serialized without blocking.
    /// The thread that serializes is normally the actor worker thread.
    /// When calling FlushAsync, the message is sent to underlying socket buffer. The task continues when the socket is ready for the next message.
    /// The stream.Position property gets the currently buffered byte count. It must be reset to 0 after flush.
    /// </summary>
    public class TcpStreamOutgoing : MemoryStream, IDisposable
    {
        private Func<byte[], int, Task> _sendAsync;

        /// <summary>
        /// Initializes a writable stream. Sending to the socket must be implemented in the callback function sendAsync.
        /// FlushAsync calls back on 'sendAsync' with the internal byte buffer and the count of bytes to send as parameters. 
        /// 'sendAsync' must return a Task for the asynchronous processing.
        /// </summary>
        /// <param name="sendAsync">FlushAsync calls back on 'sendAsync' with the internal byte buffer and the count of bytes to send as parameters. 
        /// 'sendAsync' must return a Task for the asynchronous processing.</param>
        /// <param name="initialBufferSize">Defines the initial write buffer capacity. The buffer expands up to the size of the largest message.</param>
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
        /// After a message has been serialized into the underlying memory stream, FlushAsync must be called to copy it to the TCP socket.
        /// After the async send has completed, the stream property 'Position' must be set to 0. It may not be incremented in the meantime.
        /// In .NET 4.0 FlushAsync is not available in the Stream base class. Therefore, we implement it as virtual.
        /// </summary>
        public virtual Task FlushAsync()
        {
            return _sendAsync(GetBuffer(), /*count=*/(int)Position);
        }
    }
}
