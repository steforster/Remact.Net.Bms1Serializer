namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
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
            Internal.WriteDataUInt(Bms1Tag.UByte, data);
        }

        public void WriteByteArray(byte[] data)
        {
            Internal.WriteDataLength(Bms1Tag.UByte, data.Length);
            Stream.Write(data);
        }
        
        public void WriteInt(int data)
        {
            Internal.WriteDataSInt(Bms1Tag.SInt, data);
        }
        
        public void WriteUnicode(char data)
        {
            Stream.Write((byte)Bms1Tag.String + Bms1LengthSpec.L2);
            Stream.Write((Int16)data);
        }
        
        public void WriteString(string data)
        {
            Internal.WriteDataString(Bms1Tag.String, data);
        }

    }
}
