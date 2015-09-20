namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.IO;
    using Remact.Net.Bms1Serializer.Internal;
    
    public class Bms1MessageSerializer
    {
        private Stream _readStream;
        private Bms1Reader _reader;
        private IMessageReader _messageReader;
        
        private Stream _writeStream;
        private Bms1Writer _writer;
        private IMessageWriter _messageWriter;
        
        // returns next message block type
        public int ReadMessageStart(Stream stream)
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
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadMessage(IBms1Dto messageDto)
        {
            if (_reader == null)
            {
                throw new InvalidOperationException("ReadMessageStart must be called first");
            }
            
            return _messageReader.ReadMessage(
                                    () => messageDto.Bms1Read(_reader)
                                  );
        }

                
        public void WriteMessage(Stream stream, int blockTypeId, IBms1Dto messageDto)
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

            _messageWriter.WriteMessage(_writer.Stream, blockTypeId, 
                                      () => messageDto.Bms1Write (_writer)
                                  );
        }
    }
}

