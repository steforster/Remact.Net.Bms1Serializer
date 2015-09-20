namespace Remact.Net.Bms1Serializer.Internal
{
    using System.IO;
    using Remact.Net.Bms1Serializer.Internal;

    internal class Bms1Writer : IBms1Writer
    {
        internal BinaryWriter Stream;

        private IMessageWriter _messageWriter;

        public IBms1InternalWriter Internal {get; private set;}
        
        public Bms1Writer(IBms1InternalWriter internalWriter, IMessageWriter messageWriter)
        {
            Internal = internalWriter;
            _messageWriter = messageWriter;
        }


        public void WriteByte(byte data)
        {
            if (data == 0)
            {
                Stream.Write((byte)Bms1Tag.UByte);
            }
            else
            {
                Stream.Write((byte)(Bms1Tag.UByte+1));
                Stream.Write((byte)data);
            }
        }


    }
}
