namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents the tag with attributes and datalength. 
    /// Does not include the data.
    /// Supports basic reading and writing functions.
    /// </summary>
    internal class TagReader
    {
        private int _lengthSpecifier; // = last digit of tag byte: 0..7

        private byte[] _byteBuffer;

        
        // Tag info
        public int TagByte;

        public Bms1Tag TagEnum;
        
        public bool IsArrayData;  // DataLength(in bytes) contains an array of TagType

        public int DataLength; // -2 = zero terminated, -3 = undefined length

        public int BlockTypeId; // after BlockStart or NullBlock or BaseBlockDefinition, -1 when no type id available

        public uint Checksum; // after BlockEnd
        public bool ChecksumAvailable;


        // Attributes collected for next value
        public string ObjectName;   // null = no name

        public string ObjectType;

        public int CollectionElementCount;    // -1 = no collection, -2 = collection until end of block (not a predefined length)

        public int CollectionElementBaseType; // -1 = no collection or no base type

        public List<KeyValuePair<string, string>> KeyValue;

        public List<KeyValuePair<string, string>> Namespace;

        public int TagSetNumber;    // only last attribute is used, no combinations supported

        public bool IsCharacterType;

        internal void ClearAttributes()
        {
            TagEnum = Bms1Tag.Attribute;
            ObjectName = null;
            ObjectType = null;
            CollectionElementCount = Bms1Length.None; // -1
            CollectionElementBaseType = -1;
            KeyValue = null;
            Namespace = null;
            TagSetNumber = 0;
            IsCharacterType = false;
        }


        public void ReadTag(BinaryReader stream)
        {
            TagEnum = Bms1Tag.UnknownValue;
            IsArrayData = false;
            DataLength = 0;
            _lengthSpecifier = 0;
            BlockTypeId = -1;
            ChecksumAvailable = false;

            TagByte = stream.ReadByte(); // 0..255
            if (TagByte < 10)
            {
                if (TagByte == 0)
                {
                    throw new Bms1Exception("invalid value tag 0");
                }
                else if (TagByte == 7) TagEnum = Bms1Tag.Null;
                else if (TagByte == 8) TagEnum = Bms1Tag.BoolFalse;
                else if (TagByte == 9) TagEnum = Bms1Tag.BoolTrue;
            }
            else if (TagByte < 170) // Values
            {
                EvaluateDataLength(stream);
                if (DataLength == Bms1Length.Undefined)
                {
                    throw new Bms1Exception("undefined data length specifier 3");
                }

                if (TagByte < 150)
                {
                    // Known value tags
                    TagEnum = (Bms1Tag)(TagByte - _lengthSpecifier);
                }
            }
            else if (TagByte < 245) // Attributes --> skip unused data
            {
                EvaluateDataLength(stream);

                TagEnum = Bms1Tag.Attribute;
                var attributeGroup = TagByte; // special handling for 240 and above
                if (TagByte < 240)
                {
                    attributeGroup -= _lengthSpecifier; // normal groups below 240
                }

                switch (attributeGroup)
                {
                    case 170:
                        ObjectName = ReadDataString(stream);
                        break;

                    case 180:
                        ObjectType = ReadDataString(stream);
                        break;

                    case 190:
                        if (KeyValue == null)
                        {
                            KeyValue = new List<KeyValuePair<string, string>>();
                        }
                        var kvPair = new KeyValuePair<string, string>(ReadDataString(stream), ReadDataString(stream));
                        KeyValue.Add(kvPair);
                        break;

                    case 200:
                        if (Namespace == null)
                        {
                            Namespace = new List<KeyValuePair<string, string>>();
                        }
                        var nsPair = new KeyValuePair<string, string>(ReadDataString(stream), ReadDataString(stream));
                        Namespace.Add(nsPair);
                        break;

                    case 230:
                        if (_lengthSpecifier == 3) // 233
                        {
                            CollectionElementCount = Bms1Length.Open; // open collection
                        }
                        else if (_lengthSpecifier <= 4) // 230, 231, 232, 234
                        {
                            DataLength = _lengthSpecifier;
                            var len = ReadDataUInt(stream);
                            if (len > Int32.MaxValue) // TODO Max
                            {
                                throw new Bms1Exception("collection length " + len + " out of bounds for attribute " + TagByte);
                            }
                            CollectionElementCount = (int)len; // >=0: CollectionAtribute is set.
                        }
                        else
                        {
                            SkipData(stream); // yet unknown attributes
                        }
                        break;

                    case 240: IsCharacterType = true; break;
                    case 241: TagSetNumber = 1; break;
                    case 242: TagSetNumber = 2; break;
                    case 243: TagSetNumber = 3; break;
                    case 244: break;

                    default:
                        SkipData(stream);
                        break;
                }
            }
            else // >= 245: Framing tags with specific data length
            {
                switch (TagByte)
                {
                    case 245:
                        TagEnum = Bms1Tag.MessageStart;
                        var pattern = stream.ReadUInt32();
                        if (pattern != 0x544D4201) // decimal data [01, 66, 77, 83]
                        {
                            throw new Bms1Exception(string.Format("invalid message start pattern 0x{0:X}", pattern));
                        }
                        break;

                    case 246: TagEnum = Bms1Tag.BlockStart; break;
                    case 247: TagEnum = Bms1Tag.BlockStart; BlockTypeId = stream.ReadUInt16(); break;
                    case 248: TagEnum = Bms1Tag.NullBlock; BlockTypeId = stream.ReadUInt16(); break;
                    case 249: TagEnum = Bms1Tag.BlockEnd; break;
                    case 250: TagEnum = Bms1Tag.BlockEnd; Checksum = stream.ReadUInt32(); ChecksumAvailable = true; break;
                    case 251: TagEnum = Bms1Tag.MessageFooter; break;
                    case 252: TagEnum = Bms1Tag.MessageEnd; break;
                    case 253: stream.ReadUInt32(); break;
                    case 254: stream.ReadUInt32(); break;

                    default:
                        throw new Bms1Exception("invalid framing tag = " + TagByte); // 255
                }
            }
        }


        private void EvaluateDataLength(BinaryReader stream)
        {
            _lengthSpecifier = TagByte % 10; // 0..9
            switch (_lengthSpecifier)
            {
                case 3:
                    DataLength = Bms1Length.Undefined;
                    break;

                case Bms1LengthSpec.ZeroTerminated: // 5
                    IsArrayData = true;
                    DataLength = Bms1Length.Open; // zero terminated string
                    break;

                case Bms1LengthSpec.Byte: // 6
                    IsArrayData = true;
                    DataLength = stream.ReadByte();
                    break;

                case Bms1LengthSpec.Int32: // 7
                    IsArrayData = true;
                    DataLength = stream.ReadInt32();
                    if (DataLength < 0)
                    {
                        throw new Bms1Exception("invalid data length = " + DataLength + " for tag = " + TagByte);
                    }
                    break;

                case Bms1LengthSpec.L16: // 9
                    DataLength = 16;
                    break;

                default:
                    DataLength = _lengthSpecifier; // 0, 1, 2, 4, 8
                    break;
            }
        }

        // supports UTF8 zero terminated or length defined string data of any tag type.
        // Keeps a single buffer of maximum read length. Create a new Bms1Tag to free the buffer memory.
        public string ReadDataString(BinaryReader stream)
        {
            var len = DataLength;
            DataLength = 0;
            if (_lengthSpecifier == Bms1LengthSpec.L0 || len == 0)
            {
                return string.Empty;
            }

            if (_lengthSpecifier == Bms1LengthSpec.L1)
            {
                var b = stream.ReadByte();
                return Convert.ToString((char)b);
            }

            if (_lengthSpecifier == Bms1LengthSpec.L2)
            {
                short i = stream.ReadInt16();
                return Convert.ToString((char)i);
            }

            if (_lengthSpecifier == Bms1LengthSpec.ZeroTerminated || len == Bms1Length.Open)
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

            var buffer = stream.ReadBytes(len);
            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }


        public uint ReadDataUInt(BinaryReader stream)
        {
            var len = DataLength;
            DataLength = 0;
            switch (len)
            {
                case 0: return 0;
                case 1: return stream.ReadByte();
                case 2: return stream.ReadUInt16();
                case 4: return stream.ReadUInt32();
                default:
                    // ? stream.ReadBytes(len); // skip unknown data
                    throw new Bms1Exception("invalid data length for UInt: " + len);
            }
        }


        public void SkipData(BinaryReader stream)
        {
            var len = DataLength;
            DataLength = 0; // do not read again
            if (len == 0)
            {
                return; // no data to skip
            }

            if (len > 0)
            {
                stream.ReadBytes(len); // skip unknown data
                return;
            }

            if (len == Bms1Length.Open)
            {
                // skip zero terminated string
                byte nextByte;
                do
                {
                    nextByte = stream.ReadByte();
                }
                while (nextByte != 0);
            }

            throw new Bms1Exception("undefined datalength " + len + " to skip.");
        }
    }
}
