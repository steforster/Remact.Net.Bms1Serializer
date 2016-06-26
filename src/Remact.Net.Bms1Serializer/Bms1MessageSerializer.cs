namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Remact.Net.Bms1Serializer.Internal;
    
    /// <summary>
    /// Main class to serialize or dezerialize a BMS message stream.
    /// </summary>
    public class Bms1MessageSerializer
    {
        private Stream _readStream;
        private Bms1Reader _reader;
        private IMessageReader _messageReader;
        
        private Stream _writeStream;
        private Bms1Writer _writer;
        private IMessageWriter _messageWriter;
        
        /// <summary>
        /// Reads until a message start is found. See e.g. <see cref="IBms1InternalReader.ObjectType"/> for the incoming message type.
        /// </summary>
        /// <param name="stream">A Remact.Net.Tcp.Stream or another stream to read the message from.</param>
        /// <returns>Attributes of the incoming message.</returns>
        public IBms1InternalReader ReadMessageStart(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            if (_reader == null)
            {
                var internalReader = new InternalReader();
                _messageReader = internalReader;
                _reader = new Bms1Reader(internalReader, _messageReader);
            }
            
            if (!object.ReferenceEquals(_readStream, stream))
            {
                _readStream = stream;
                _reader.Stream = new System.IO.BinaryReader(_readStream);
            }

            return _messageReader.ReadMessageStart(_reader.Stream);
        }

        /// <summary>
        /// Read the message after <see cref="ReadMessageStart"/> has been called.
        /// Skips all message data and returns null, when null is passed as readMessageDto.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="readMessageDto">A function that deserializes a message of (base) type T.</param>
        /// <returns>The message of (base) Type T.</returns>
        public T ReadMessage<T>(Func<IBms1Reader, T> readMessageDto) where T : new()
        {
            if (_reader == null)
            {
                throw new InvalidOperationException("ReadMessageStart must be called first");
            }

            return _messageReader.ReadMessage<T>(_reader, readMessageDto);
        }

        
        /// <summary>
        /// Writes a message to the stream. Blocks until message has been serialized and flushed to the output stream.
        /// </summary>
        /// <param name="stream">A Remact.Net.Tcp.Stream or another stream to write the message to.</param>
        /// <param name="writeDtoAction">A lambda expression that serializes a message of type T.</param>
                     
        public void WriteMessage(Stream stream, Action<IBms1Writer> writeDtoAction)
        {
            CheckStream (stream);
            _messageWriter.WriteMessage(_writer, writeDtoAction);
            stream.Flush(); // TODO cancellation token
        }

        
        /// <summary>
        /// Writes a message to the stream.
        /// </summary>
        /// <param name="stream">A Remact.Net.Tcp.Stream or another stream to write the message to.</param>
        /// <param name="writeDtoAction">A lambda expression that serializes a message of type T.</param>
        /// <returns>A task that completes after the message has been flushed to the output stream.</returns>
                      
        public async Task WriteMessageAsync(Stream stream, Action<IBms1Writer> writeDtoAction)
        {
            CheckStream (stream);
            _messageWriter.WriteMessage(_writer, writeDtoAction);
            await stream.FlushAsync(); // TODO cancellation token
        }

        private void CheckStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            if (_writer == null)
            {
                var internalWriter = new InternalWriter();
                _messageWriter = internalWriter;
                _writer = new Bms1Writer(internalWriter, _messageWriter);
            }
            
            if (!object.ReferenceEquals(_writeStream, stream))
            {
                _writeStream = stream;
                _writer.Stream = new System.IO.BinaryWriter(_writeStream);
            }
        }
    }
}

