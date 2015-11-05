namespace Remact.Net.Bms1Serializer.Internal
{
    using System.IO;
    
    /// <summary>
    /// Used for unit testing instead of Bms1MessageSerializer
    /// </summary>
    public class TestSerializer
    {
        private Bms1Reader _reader;
        private Bms1Writer _writer;
        
        public IBms1Reader Reader
        {
            get
            {
                return _reader;
            }
        }

        public IBms1Writer Writer
        {
            get
            {
                return _writer;
            }
        }

        public TestSerializer(Stream stream)
        {
            var internalReader = new InternalReader();
            _reader = new Bms1Reader(internalReader, internalReader);
            _reader.Stream = new System.IO.BinaryReader(stream);
            internalReader.Stream = _reader.Stream;

            var internalWriter = new InternalWriter();
            _writer = new Bms1Writer(internalWriter, internalWriter);
            _writer.Stream = new System.IO.BinaryWriter(stream);
            internalWriter.Stream = _writer.Stream;
        }
    }
}

