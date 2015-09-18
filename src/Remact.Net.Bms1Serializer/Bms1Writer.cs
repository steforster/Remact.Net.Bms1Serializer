namespace Remact.Net.Bms1Serializer
{
    using System.IO;
    using Remact.Net.Bms1Serializer.Internal;

    public class Bms1Writer
    {
        private BinaryWriter _stream;

        private Attributes _attributes;

        private TagReader _tag;


        public Bms1Writer(BinaryWriter streamWriter)
        {
            _stream = streamWriter;
            _tag = new TagReader();
            _attributes = new Attributes();
            EndOfMessage = true;
            EndOfBlock = true;
        }

        public bool EndOfMessage { get; private set; } // before message start or after message end
        public bool EndOfBlock   { get; private set; } // before block start or after block end

    }
}
