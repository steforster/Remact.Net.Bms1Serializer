namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.IO;
    using System.Text;
    using Remact.Net.Bms1Serializer.Internal;

    public class Bms1Reader : IBms1Reader
    {
        private BinaryReader _stream;
        private IBms1MessageReader _messageReader;
        // private Encoding _asciiEncoding;// ASCII Encoding not available in PortableFramework

        public  IBms1InternalReader Internal {get; private set;}


        public Bms1Reader(BinaryReader binaryReader)
        {
            _stream = binaryReader;
            var temp = new InternalReader(binaryReader);
            Internal = temp;
            _messageReader = temp;
        }

        
        // returns next message block type
        public int ReadMessageStart()
        {
            return _messageReader.ReadMessageStart();
        }

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadMessage(IBms1Dto messageDto)
        {
            return _messageReader.ReadMessage(() => messageDto.Bms1Read(this, Internal.BlockTypeId));
        }

        // returns next block type
        public int ReadBlockStart()
        {
            return _messageReader.ReadBlockStart();
        }
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadBlock(IBms1Dto blockDto)
        {
            return _messageReader.ReadBlock(() => blockDto.Bms1Read(this, Internal.BlockTypeId));
        }
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadBool(ref bool data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.DataLength == 0 && !Internal.IsArrayData)
            {
                if (Internal.TypeTag == Bms1Tag.BoolFalse)
                {
                    data = false;
                    return true;
                }
                else if (Internal.TypeTag == Bms1Tag.BoolTrue)
                {
                    data = true;
                    return true;
                }
            }
            throw new Bms1Exception("cannot read bool");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByte(ref byte data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && !Internal.IsArrayData)
            {
                if (Internal.DataLength == 0)
                {
                    data = 0;
                    return true;
                }
                else if (Internal.DataLength == 1)
                {
                    data = _stream.ReadByte();
                    return true;
                }
            }
            throw new Bms1Exception("cannot read byte");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByteArray(ref byte[] data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && Internal.IsArrayData)
            {
                data = _stream.ReadBytes(Internal.DataLength);
                return true;
            }
            throw new Bms1Exception("cannot read byte array");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadInt(ref int data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.SInt && !Internal.IsArrayData)
            {
                switch (Internal.DataLength)
                {
                    case 0: data = 0; return true;
                    case 1: data = _stream.ReadSByte(); return true;
                    case 2: data = _stream.ReadInt16(); return true;
                    case 4: data = _stream.ReadInt32(); return true;
                }
            }
            throw new Bms1Exception("cannot read int");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadString(ref string data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.String && Internal.IsArrayData)
            {
                data = Internal.ReadDataString();
                return true;
            }

            if (Internal.TypeTag == Bms1Tag.UByte && Internal.IsArrayData && Internal.IsCharacterType)
            {
                var buffer = _stream.ReadBytes(Internal.DataLength);
                //data = _asciiEncoding.GetString(buffer, 0, buffer.Length);
                data = Encoding.UTF8.GetString(buffer, 0, buffer.Length); // fallback to UTF8 because ASCII is not available in portable framework.
                return true;
            }

            if (Internal.TypeTag == Bms1Tag.UShort && Internal.IsArrayData && Internal.IsCharacterType)
            {
                var buffer = _stream.ReadBytes(Internal.DataLength);
                data = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                return true;
            }

            throw new Bms1Exception("cannot read string");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadChar(ref char data)
        {
            if (!Internal.ReadAttributes())
            {
                return false;
            }

            if (Internal.TypeTag == Bms1Tag.String && !Internal.IsArrayData)
            {
                return ConvertToChar(ref data);
            }

            if (Internal.TypeTag == Bms1Tag.UByte && !Internal.IsArrayData && Internal.IsCharacterType)
            {
                return ConvertToChar(ref data);
            }

            if (Internal.TypeTag == Bms1Tag.UShort && !Internal.IsArrayData && Internal.IsCharacterType)
            {
                return ConvertToChar(ref data);
            }

            throw new Bms1Exception("cannot read char");
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
                short b = _stream.ReadByte();
                data = (char)b;
                return true;
            }
            else if  (Internal.DataLength == 2)
            {
                data = (char)_stream.ReadInt16();
            }
            return false;
        }
    }
}
