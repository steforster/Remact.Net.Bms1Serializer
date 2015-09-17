namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal enum Tag
    {
        // Known value tags.
        BoolFalse = 10,
        BoolTrue  = 11,
        Null      = 12,
        UByte     = 20,  //  8 bit
        UShort    = 30,  // 16 bit
        SShort    = 40,
        UInt      = 50,  // 32 bit
        SInt      = 60,
        SLong     = 70,  // 64 bit
        Enum      = 80,  //  8...64 bit
        Bitset    = 90,  //  8..128 bit
        Decimal   = 100, //128 bit
        Float     = 110, // 32 bit
        Double    = 120, // 64 bit
        Date      = 130,
        Time      = 140,
        String    = 150,

        // Known framing tags.
        NullBlock           = 240,
        BaseBlockDefinition = 241,
        BlockStart          = 242,
        BlockEnd            = 244,
        Undefined           = 246,
        MessageStart        = 250,
        MessageFooter       = 253,
        MessageEnd          = 254,
        //Invalid = 255,

        // Any attribute or unknown value tag --> allowed to skip
        Attribute = 256 
    }


    /// <summary>
    /// Represents the tag with datalength. 
    /// Does not include the data.
    /// Supports basic reading and writing functions.
    /// </summary>
    internal class Bms1Tag
    {
        public Tag Type;

        public int AttributeTagType; // = tag byte with last digit set to 0, when value >= 20.

        public bool IsArrayValue;  // DataLength(in bytes) contains an array of TagType

        public int DataLength; // -1 = zero terminated
        
        public int BlockTypeId; // after BlockStart or NullBlock or BaseBlockDefinition, -1 when no type id available

        public int Checksum; // after BlockEnd, is -1 when no checksum available


        private int _lengthSpecifier; // = last digit of tag byte: 0..7

        private byte[] _byteBuffer;


        public void ReadTag(BinaryReader stream)
        {
            Type = Tag.Attribute; // attribute or unknown value tag
            IsArrayValue = false;
            DataLength = 0;
            _lengthSpecifier = 0;
            AttributeTagType = 0;

            int tagByte = stream.ReadByte(); // 0..255
            if (tagByte < 20)
            {
                if      (tagByte == 10) Type = Tag.BoolFalse;
                else if (tagByte == 11) Type = Tag.BoolTrue;
                else if (tagByte == 12) Type = Tag.Null;
                else
                {
                    AttributeTagType = tagByte;
                }
            }
            else if (tagByte < 240)
            {
                _lengthSpecifier = tagByte % 10; // 0..9
                switch (_lengthSpecifier)
                {
                    case 3:
                        throw new Bms1Exception("unknown data length specifier 3");

                    case 5:
                        IsArrayValue = true;
                        DataLength = -1; // zero terminated string
                        break;

                    case 6:
                        IsArrayValue = true;
                        DataLength = stream.ReadByte();
                        break;

                    case 7: 
                        IsArrayValue = true;
                        DataLength = stream.ReadInt32();
                        if (DataLength < 0)
                        {
                            throw new Bms1Exception("unknown data length = " + DataLength);
                        }
                        break;

                    case 9: 
                        DataLength = 16;
                        break;

                    default:
                        DataLength = _lengthSpecifier; // 0, 1, 2, 4, 8
                        break;
                }

                if (tagByte < 160)
                {
                    // Known value tags, DataLength is ready
                    Type = (Tag)(tagByte - _lengthSpecifier);
                }
                else
                {
                    // Known or unknown attribute- or value tag, DataLength is ready
                    AttributeTagType = (tagByte - _lengthSpecifier); // length specifier is set to 0 in AttributeTagType
                }
            }
            else
            {
                BlockTypeId = -1;
                Checksum = -1; 
                // Known framing tag with specific data length
                switch (tagByte)
                {
                    case 240: Type = Tag.NullBlock; BlockTypeId = stream.ReadUInt16(); break;
                    case 241: Type = Tag.BaseBlockDefinition; BlockTypeId = stream.ReadUInt16(); break;
                    case 242: Type = Tag.BlockStart; break;
                    case 243: Type = Tag.BlockStart; BlockTypeId = stream.ReadUInt16(); break;
                    case 244: Type = Tag.BlockEnd; break;
                    case 245: Type = Tag.BlockEnd; Checksum = stream.ReadUInt16(); break;
                    case 246: Type = Tag.Attribute; AttributeTagType = tagByte; stream.ReadUInt32(); break;
                    case 247: Type = Tag.Attribute; AttributeTagType = tagByte; stream.ReadUInt32(); break;
                    case 248: Type = Tag.Attribute; AttributeTagType = tagByte; stream.ReadUInt32(); break;
                    case 249: Type = Tag.Attribute; AttributeTagType = tagByte; stream.ReadUInt32(); break;
                    case 251: Type = Tag.MessageStart; break;
                    case 253: Type = Tag.MessageFooter; break;
                    case 254: Type = Tag.MessageEnd; break;

                    case 250: Type = Tag.MessageStart; 
                        var pattern = stream.ReadUInt32();
                        if (pattern != 0x544D4201) // decimal data [01, 66, 77, 83]
                        {
                            throw new Bms1Exception(string.Format("invalid message start pattern 0x{0:X}", pattern));
                        }
                        break;

                    default:
                        throw new Bms1Exception("invalid framing tag = " + tagByte); // 252(version2) and 255
                }
            }
        }

        // supports UTF8 zero terminated or length defined string data of any tag type.
        // Keeps a single buffer of maximum read length. Create a new Bms1Tag to free the buffer memory.
        public string ReadDataString(BinaryReader stream)
        {
            if (_lengthSpecifier == 0 || DataLength == 0)
            {
                return string.Empty;
            }

            if (_lengthSpecifier == 1)
            {
                var b = stream.ReadByte();
                return Convert.ToString((char)b);
            }

            if (_lengthSpecifier == 2)
            {
                short i = stream.ReadInt16();
                return Convert.ToString((char)i);
            }

            if (_lengthSpecifier == 5 || DataLength < 0)
            {
                if (_byteBuffer == null)
                {
                    _byteBuffer = new byte[250];
                }

                int count = 0;
                while (true)
                {
                    var b = stream.ReadByte();
                    if (b == 0)
                    {
                        // zero termination reached
                        return Encoding.UTF8.GetString(_byteBuffer, 0, count);
                    }

                    if (count >= _byteBuffer.Length)
                    {
                        var oldBuf = _byteBuffer;
                        _byteBuffer = new byte[2*count];
                        Array.Copy(oldBuf, _byteBuffer, count);
                    }

                    _byteBuffer[count++] = b;
                }
            }

            var buffer = stream.ReadBytes(DataLength);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }


        public uint ReadDataUInt(BinaryReader stream)
        {
            switch (DataLength)
            {
                case 0: return 0;
                case 1: return stream.ReadByte();
                case 2: return stream.ReadUInt16();
                case 4: return stream.ReadUInt32();
                default:
                    throw new Bms1Exception("invalid data length for UInt: " + DataLength);
            }
        }


        public void SkipData(BinaryReader stream)
        {
            if (DataLength == 0)
            {
                return; // no data to skip
            }

            if (DataLength > 0)
            {
                stream.ReadBytes(DataLength); // skip unknown data
                return;
            }

            // skip zero terminated string
            byte nextByte;
            do
            {
                nextByte = stream.ReadByte();
            }
            while (nextByte != 0);
        }
    }
}
