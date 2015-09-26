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
            throw Internal.Bms1Exception("cannot read block");
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
                throw Internal.Bms1Exception("cannot read block collection");
            }
            return true;
        }

        
        public bool ReadBool()
        {
            if (!Internal.EndOfBlock && Internal.DataLength == 0 && !Internal.IsArrayData)
            {
                if (Internal.TagEnum == Bms1Tag.BoolFalse)
                {
                    Internal.ReadAttributes(); // next tag
                    return false;
                }
                else if (Internal.TagEnum == Bms1Tag.BoolTrue)
                {
                    Internal.ReadAttributes();
                    return true;
                }
            }
            throw Internal.Bms1Exception("cannot read bool");
        }


        public byte ReadByte()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.Byte))
            {
                if (Internal.DataLength == 0)
                {
                    Internal.ReadAttributes();
                    return 0;
                }
                else if (Internal.DataLength == 1)
                {
                    var data = Stream.ReadByte();
                    Internal.ReadAttributes();
                    return data;
                }
            }
            throw Internal.Bms1Exception("cannot read byte");
        }


        public byte[] ReadByteArray()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Byte && Internal.IsArrayData)
                {
                    var data = Stream.ReadBytes(Internal.DataLength);
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read byte array");
        }


        public Int16 ReadInt16()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.Int16))
            {
                Int16 data;
                switch (Internal.DataLength)
                {
                    case 0: Internal.ReadAttributes(); return 0;
                    case 1: data = Stream.ReadSByte(); Internal.ReadAttributes(); return data;
                    case 2: data = Stream.ReadInt16(); Internal.ReadAttributes(); return data;
                }
            }
            throw Internal.Bms1Exception("cannot read Int16");
        }


        public Int16[] ReadInt16Array()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Int16 && Internal.IsArrayData)
                {
                    var count = Internal.DataLength / 2;
                    var data = new Int16[count];
                    for (int i = 0; i < count; i++)
                    {
                        data[i] = Stream.ReadInt16();
                    }
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read Int16 array");
        }


        public Int32 ReadInt32()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.Int32))
            {
                Int32 data;
                switch (Internal.DataLength)
                {
                    case 0: Internal.ReadAttributes(); return 0;
                    case 1: data = Stream.ReadSByte(); Internal.ReadAttributes(); return data;
                    case 2: data = Stream.ReadInt16(); Internal.ReadAttributes(); return data;
                    case 4: data = Stream.ReadInt32(); Internal.ReadAttributes(); return data;
                }
            }
            throw Internal.Bms1Exception("cannot read Int32");
        }


        public Int32[] ReadInt32Array()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Int32 && Internal.IsArrayData)
                {
                    var count = Internal.DataLength / 4;
                    var data = new Int32[count];
                    for (int i = 0; i < count; i++)
                    {
                        data[i] = Stream.ReadInt32();
                    }
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read Int32 array");
        }


        public Int64 ReadInt64()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.Int64))
            {
                Int64 data;
                switch (Internal.DataLength)
                {
                    case 0: Internal.ReadAttributes(); return 0;
                    case 1: data = Stream.ReadSByte(); Internal.ReadAttributes(); return data;
                    case 2: data = Stream.ReadInt16(); Internal.ReadAttributes(); return data;
                    case 4: data = Stream.ReadInt32(); Internal.ReadAttributes(); return data;
                    case 8: data = Stream.ReadInt64(); Internal.ReadAttributes(); return data;
                }
            }
            throw Internal.Bms1Exception("cannot read Int64");
        }


        public Int64[] ReadInt64Array()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Int64 && Internal.IsArrayData)
                {
                    var count = Internal.DataLength / 8;
                    var data = new Int64[count];
                    for (int i = 0; i < count; i++)
                    {
                        data[i] = Stream.ReadInt64();
                    }
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read Int64 array");
        }


        public UInt16 ReadUInt16()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.UInt16))
            {
                UInt16 data;
                switch (Internal.DataLength)
                {
                    case 0: Internal.ReadAttributes(); return 0;
                    case 1: data = Stream.ReadByte(); Internal.ReadAttributes(); return data;
                    case 2: data = Stream.ReadUInt16(); Internal.ReadAttributes(); return data;
                }
            }
            throw Internal.Bms1Exception("cannot read UInt16");
        }


        public UInt16[] ReadUInt16Array()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.UInt16 && Internal.IsArrayData)
                {
                    var count = Internal.DataLength / 2;
                    var data = new UInt16[count];
                    for (int i = 0; i < count; i++)
                    {
                        data[i] = Stream.ReadUInt16();
                    }
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read UInt16 array");
        }


        public UInt32 ReadUInt32()
        {
            if (Internal.IsSingleValueOfType(Bms1Tag.UInt32))
            {
                UInt32 data = Internal.ReadDataUInt();
                Internal.ReadAttributes(); 
                return data;
            }
            throw Internal.Bms1Exception("cannot read UInt32");
        }


        public UInt32[] ReadUInt32Array()
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.UInt32 && Internal.IsArrayData)
                {
                    var count = Internal.DataLength / 4;
                    var data = new UInt32[count];
                    for (int i = 0; i < count; i++)
                    {
                        data[i] = Stream.ReadUInt32();
                    }
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read UInt32 array");
        }


        public string ReadString()
        {
            if (!Internal.EndOfBlock)
            {
                string data;
                if (Internal.TagEnum == Bms1Tag.Char)
                {
                    if (Internal.IsArrayData)
                    {
                        data = Internal.ReadDataString();
                        Internal.ReadAttributes();
                        return data;
                    }
                    else if (Internal.DataLength == 0)
                    {
                        Internal.ReadAttributes();
                        return string.Empty;
                    }
                }

                if (Internal.TagEnum == Bms1Tag.Byte && Internal.IsArrayData && Internal.IsCharacterType)
                {
                    var buffer = Stream.ReadBytes(Internal.DataLength);
                    //data = _asciiEncoding.GetString(buffer, 0, buffer.Length);
                    data = Encoding.UTF8.GetString(buffer, 0, buffer.Length); // fallback to UTF8 because ASCII is not available in portable framework.
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.UInt16 && Internal.IsArrayData && Internal.IsCharacterType)
                {
                    var buffer = Stream.ReadBytes(Internal.DataLength);
                    data = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                    Internal.ReadAttributes();
                    return data;
                }

                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }
            }
            throw Internal.Bms1Exception("cannot read string");
        }


        public char ReadChar()
        {
            var ok = false;
            char data = '\0';
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Char && !Internal.IsArrayData)
                {
                    ok = ConvertToChar(ref data);
                }
                else if (Internal.TagEnum == Bms1Tag.Byte && !Internal.IsArrayData && Internal.IsCharacterType)
                {
                    ok = ConvertToChar(ref data);
                }
                else if (Internal.TagEnum == Bms1Tag.UInt16 && !Internal.IsArrayData && Internal.IsCharacterType)
                {
                    ok = ConvertToChar(ref data);
                }
            }
            
            if (!ok)
            {
                throw Internal.Bms1Exception("cannot read char");
            }
            Internal.ReadAttributes();
            return data;
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
                return true;
            }
            return false;
        }


        public Nullable<T> ReadNullable<T>() where T : struct
        {
            if (!Internal.EndOfBlock)
            {
                if (Internal.TagEnum == Bms1Tag.Null)
                {
                    Internal.ReadAttributes();
                    return null;
                }

                object data = null;
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean: data = ReadBool(); break;
                    case TypeCode.Byte:    data = ReadByte(); break;
                    case TypeCode.Char:    data = ReadChar(); break;
                    case TypeCode.Int16:   data = ReadInt16(); break;
                    case TypeCode.Int32:   data = ReadInt32(); break;
                    case TypeCode.Int64:   data = ReadInt64(); break;
                    case TypeCode.UInt16:  data = ReadUInt16(); break;
                    case TypeCode.UInt32:  data = ReadUInt32(); break;
                }

                if (data != null)
                {
                    return (T)data;
                }
            }
            throw Internal.Bms1Exception("cannot read Nullable<" + typeof(T).Name + ">");
        }
    }
}
