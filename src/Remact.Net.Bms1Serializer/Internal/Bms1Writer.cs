namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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

        public void WriteNull()
        {
            Internal.WriteDataLength(Bms1Tag.Null, -2); // writes attributes previously, does not modify the tag and writes no data length.
        }

        public void WriteBool(bool data)
        {
            if (data)
            {
                Internal.WriteDataLength(Bms1Tag.BoolTrue, -2); // writes attributes previously, does not modify the tag and writes no data length.
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.BoolFalse, -2);
            }
        }

        public void WriteByte(byte data)
        {
            Internal.WriteDataUInt(Bms1Tag.UByte, data); // writes attributes previously
        }

        public void WriteByteArray(byte[] data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.UByte, data.Length); // writes attributes previously
                Stream.Write(data);
            }
        }

        public void WriteByteArray(IEnumerable<Byte> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.UByte, data.Count());
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        public void WriteUInt16(UInt16 data)
        {
            Internal.WriteDataUInt(Bms1Tag.UInt16, data);
        }

        public void WriteUInt16Array(IEnumerable<UInt16> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.UInt16, data.Count() * 2);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        public void WriteUInt32(UInt32 data)
        {
            Internal.WriteDataUInt(Bms1Tag.UInt32, data);
        }

        public void WriteUInt32Array(IEnumerable<UInt32> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.UInt32, data.Count() * 4);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        public void WriteInt16(Int16 data)
        {
            Internal.WriteDataSInt(Bms1Tag.SInt16, data); // writes attributes previously
        }

        public void WriteInt16Array(IEnumerable<Int16> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.SInt16, data.Count() * 2);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        public void WriteInt(int data)
        {
            Internal.WriteDataSInt(Bms1Tag.SInt32, data); // writes attributes previously
        }

        public void WriteIntArray(IEnumerable<int> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.SInt32, data.Count() * 4);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }


        public void WriteInt64(Int64 data)
        {
            Internal.WriteDataSInt64(Bms1Tag.SInt64, data);
        }

        public void WriteInt64Array(IEnumerable<Int64> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataLength(Bms1Tag.SInt64, data.Count() * 8);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        public void WriteUnicode(char data)
        {
            Internal.WriteDataLength(Bms1Tag.Char + Bms1LengthSpec.L2, -2); // writes attributes previously, does not modify the tag and writes no data length.
            Stream.Write((Int16)data);
        }
        
        public void WriteString(string data)
        {
            Internal.WriteDataString(Bms1Tag.Char, data); // supports null
        }

    }
}
