namespace Remact.Net.Bms1Serializer
{
    using System.IO;

    public class Bms1Writer
    {
        private BinaryWriter _stream;

        private Bms1Attributes _attributes;

        private Bms1Tag _tag;


        public Bms1Writer(BinaryWriter streamWriter)
        {
            _stream = streamWriter;
            _tag = new Bms1Tag();
            _attributes = new Bms1Attributes();
            EndOfMessage = true;
            EndOfBlock = true;
        }

        public bool EndOfMessage { get; private set; } // before message start or after message end
        public bool EndOfBlock   { get; private set; } // before block start or after block end

    }
}
