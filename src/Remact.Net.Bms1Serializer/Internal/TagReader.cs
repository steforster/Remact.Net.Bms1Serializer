namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Represents the tag with datalength. 
    /// Does not include the data.
    /// Supports basic reading and writing functions.
    /// </summary>
    internal class TagReader
    {
        public Bms1Tag TypeTag;

        public int AttributeTag; // = tag byte with last digit set to 0, when value >= 20.

        public bool IsArrayData;  // DataLength(in bytes) contains an array of TagType

        public int DataLength; // -1 = zero terminated
        
        public int BlockTypeId; // after BlockStart or NullBlock or BaseBlockDefinition, -1 when no type id available

        public int Checksum; // after BlockEnd, is -1 when no checksum available


        private int _lengthSpecifier; // = last digit of tag byte: 0..7

        private byte[] _byteBuffer;


        public void ReadTag(BinaryReader stream)
        {
            TypeTag = Bms1Tag.Attribute; // attribute or unknown value tag
            IsArrayData = false;
            DataLength = 0;
            _lengthSpecifier = 0;
            AttributeTag = 0;

            int tagByte = stream.ReadByte(); // 0..255
            if (tagByte < 20)
            {
                if      (tagByte == 10) TypeTag = Bms1Tag.BoolFalse;
                else if (tagByte == 11) TypeTag = Bms1Tag.BoolTrue;
                else if (tagByte == 12) TypeTag = Bms1Tag.Null;
                else
                {
                    AttributeTag = tagByte;
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
                        IsArrayData = true;
                        DataLength = -1; // zero terminated string
                        break;

                    case 6:
                        IsArrayData = true;
                        DataLength = stream.ReadByte();
                        break;

                    case 7: 
                        IsArrayData = true;
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
                    TypeTag = (Bms1Tag)(tagByte - _lengthSpecifier);
                }
                else
                {
                    // Known or unknown attribute- or value tag, DataLength is ready
                    AttributeTag = (tagByte - _lengthSpecifier); // length specifier is set to 0 in AttributeTagType
                }
            }
            else
            {
                BlockTypeId = -1;
                Checksum = -1; 
                // Known framing tag with specific data length
                switch (tagByte)
                {
                    case 240: TypeTag = Bms1Tag.NullBlock; BlockTypeId = stream.ReadUInt16(); break;
                    case 241: TypeTag = Bms1Tag.BaseBlockDefinition; BlockTypeId = stream.ReadUInt16(); break;
                    case 242: TypeTag = Bms1Tag.BlockStart; break;
                    case 243: TypeTag = Bms1Tag.BlockStart; BlockTypeId = stream.ReadUInt16(); break;
                    case 244: TypeTag = Bms1Tag.BlockEnd; break;
                    case 245: TypeTag = Bms1Tag.BlockEnd; Checksum = stream.ReadUInt16(); break;
                    case 246: TypeTag = Bms1Tag.Attribute; AttributeTag = tagByte; stream.ReadUInt32(); break;
                    case 247: TypeTag = Bms1Tag.Attribute; AttributeTag = tagByte; stream.ReadUInt32(); break;
                    case 248: TypeTag = Bms1Tag.Attribute; AttributeTag = tagByte; stream.ReadUInt32(); break;
                    case 249: TypeTag = Bms1Tag.Attribute; AttributeTag = tagByte; stream.ReadUInt32(); break;
                    case 251: TypeTag = Bms1Tag.MessageStart; break;
                    case 253: TypeTag = Bms1Tag.MessageFooter; break;
                    case 254: TypeTag = Bms1Tag.MessageEnd; break;

                    case 250: TypeTag = Bms1Tag.MessageStart; 
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
