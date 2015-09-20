namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;

    internal class Bms1Reader : IBms1Reader
    {
        private IMessageReader _messageReader;
        internal BinaryReader Stream;
        // private Encoding _asciiEncoding;// ASCII Encoding not available in PortableFramework

        public  IBms1InternalReader Internal {get; private set;}
        
        internal Bms1Reader(IBms1InternalReader internalReader, IMessageReader messageReader)
        {
            Internal = internalReader;
            _messageReader = messageReader;
        }

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadBlock(IBms1Dto blockDto)
        {
            if (blockDto == null)
            {
                throw new ArgumentNullException("blockDto");
            }
            if (Internal.EndOfBlock)
            {
                return false;
            }
            
            if (Internal.IsBlockType && !Internal.IsCollection)
            {
                return _messageReader.ReadBlock(() => blockDto.Bms1Read(this));
            }
            return Internal.ThrowError("cannot read block");
        }
        
        public bool ReadBlocks(Func<IBms1InternalReader, IBms1Dto> blockFactory)
        {
            if (blockFactory == null)
            {
                throw new ArgumentNullException("blockFactory");
            }
            
            int count = Internal.CollectionElementCount; // -1 = no collection attribute
            int readCount = -1;
            if (!Internal.EndOfBlock && Internal.IsBlockType && count >= 0)
            {
                readCount = 0;
                do
                {
                    var blockDto = blockFactory(Internal);
                    _messageReader.ReadBlock(() => blockDto.Bms1Read(this));
                    readCount++;
                }
                while (!Internal.EndOfBlock && Internal.IsBlockType && readCount < count);
            }
            
            if (readCount < 0)
            {
                Internal.ThrowError("cannot read block collection");
            }
            return true;
        }
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadBool(ref bool data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }

            if (Internal.DataLength == 0 && !Internal.IsArrayData)
            {
                if (Internal.TypeTag == Bms1Tag.BoolFalse)
                {
                    data = false;
                    Internal.ReadAttributes();
                    return true;
                }
                else if (Internal.TypeTag == Bms1Tag.BoolTrue)
                {
                    data = true;
                    Internal.ReadAttributes();
                    return true;
                }
            }
            return Internal.ThrowError("cannot read bool");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByte(ref byte data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && !Internal.IsArrayData)
            {
                if (Internal.DataLength == 0)
                {
                    data = 0;
                    Internal.ReadAttributes();
                    return true;
                }
                else if (Internal.DataLength == 1)
                {
                    data = Stream.ReadByte();
                    Internal.ReadAttributes();
                    return true;
                }
            }
            return Internal.ThrowError("cannot read byte");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByteArray(ref byte[] data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && Internal.IsArrayData)
            {
                data = Stream.ReadBytes(Internal.DataLength);
                Internal.ReadAttributes();
                return true;
            }
            return Internal.ThrowError("cannot read byte array");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadInt(ref int data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.SInt && !Internal.IsArrayData)
            {
                switch (Internal.DataLength)
                {
                    case 0: data = 0;                  Internal.ReadAttributes(); return true;
                    case 1: data = Stream.ReadSByte(); Internal.ReadAttributes(); return true;
                    case 2: data = Stream.ReadInt16(); Internal.ReadAttributes(); return true;
                    case 4: data = Stream.ReadInt32(); Internal.ReadAttributes(); return true;
                }
            }
            return Internal.ThrowError("cannot read int");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadString(ref string data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.String && Internal.IsArrayData)
            {
                data = Internal.ReadDataString();
                Internal.ReadAttributes();
                return true;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && Internal.IsArrayData && Internal.IsCharacterType)
            {
                var buffer = Stream.ReadBytes(Internal.DataLength);
                //data = _asciiEncoding.GetString(buffer, 0, buffer.Length);
                data = Encoding.UTF8.GetString(buffer, 0, buffer.Length); // fallback to UTF8 because ASCII is not available in portable framework.
                Internal.ReadAttributes();
                return true;
            }

            if (Internal.TypeTag == Bms1Tag.UShort && Internal.IsArrayData && Internal.IsCharacterType)
            {
                var buffer = Stream.ReadBytes(Internal.DataLength);
                data = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                Internal.ReadAttributes();
                return true;
            }

            return Internal.ThrowError("cannot read string");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadChar(ref char data)
        {
            if (Internal.EndOfBlock)
            {
                return false;
            }
            
            var ok = false;
            if (Internal.TypeTag == Bms1Tag.String && !Internal.IsArrayData)
            {
                ok = ConvertToChar(ref data);
            }
            else if (Internal.TypeTag == Bms1Tag.UByte && !Internal.IsArrayData && Internal.IsCharacterType)
            {
                ok = ConvertToChar(ref data);
            }
            else if (Internal.TypeTag == Bms1Tag.UShort && !Internal.IsArrayData && Internal.IsCharacterType)
            {
                ok = ConvertToChar(ref data);
            }
            
            if (!ok)
            {
                Internal.ThrowError("cannot read char");
            }
            Internal.ReadAttributes();
            return true;
        }

        private bool ConvertToChar(ref char data)
        {
            if (Internal.DataLength == 0)
            {
                data = '\0';
                return true;
            }
            else if (Internal.DataLength == 0)
            {   // convert one ASCII byte to unicode char
                //byte[] buffer = new byte[1];
                //buffer[0] = _stream.ReadByte();
                //var s = _asciiEncoding.GetString(buffer, 0, 1);
                //data = s[0];
                // ASCII Encoding not available in PortableFramework
                short b = Stream.ReadByte();
                data = (char)b;
                return true;
            }
            else if  (Internal.DataLength == 2)
            {
                data = (char)Stream.ReadInt16();
            }
            return false;
        }
    }
}
