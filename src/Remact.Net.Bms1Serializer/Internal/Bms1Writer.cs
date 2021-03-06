﻿namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

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
            Internal.WriteAttributesAndTag(Bms1Tag.Null);
        }

        //------------------------------
        public void WriteBlock(UInt16 blockTypeId, Action writeDtoAction)
        {
            if (writeDtoAction == null)
            {
                WriteNull();
            }
            else
            {
                _messageWriter.WriteBlock(this, blockTypeId, writeDtoAction);
            }
        }

        public void WriteBlock(Action writeDtoAction)
        {
            if (writeDtoAction == null)
            {
                WriteNull();
            }
            else
            {
                _messageWriter.WriteBlock(this, -1, writeDtoAction);
            }
        }

        public void WriteBlocks<T>(UInt16 baseBlockTypeId, IEnumerable<T> data, Action<object, IBms1Writer> writeBlockAction)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.CollectionElementCount = data.Count();
                _messageWriter.WriteBlock(this, baseBlockTypeId, () =>
                    {
                        foreach (T block in data)
                        {
                            writeBlockAction((object)block, this);
                        }
                    });
            }
        }

        //------------------------------
        public void WriteBool(bool data)
        {
            if (data)
            {
                Internal.WriteAttributesAndTag(Bms1Tag.BoolTrue);
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.BoolFalse);
            }
        }

        public void WriteBool(bool? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                WriteBool(data.Value);
            }
        }

        //------------------------------
        public void WriteByte(byte data)
        {
            Internal.WriteDataUInt(Bms1Tag.Byte, data); // writes attributes previously
        }

        public void WriteByte(byte? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataUInt(Bms1Tag.Byte, data.Value);
            }
        }

        public void WriteByteArray(byte[] data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.Byte, data.Length);
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
                Internal.WriteAttributesAndTag(Bms1Tag.Byte, data.Count());
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteUInt16(UInt16 data)
        {
            Internal.WriteDataUInt(Bms1Tag.UInt16, data);
        }

        public void WriteUInt16(UInt16? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataUInt(Bms1Tag.UInt16, data.Value);
            }
        }

        public void WriteUInt16Array(IEnumerable<UInt16> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.UInt16, data.Count() * 2);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteUInt32(UInt32 data)
        {
            Internal.WriteDataUInt(Bms1Tag.UInt32, data);
        }

        public void WriteUInt32(UInt32? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataUInt(Bms1Tag.UInt32, data.Value);
            }
        }

        public void WriteUInt32Array(IEnumerable<UInt32> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.UInt32, data.Count() * 4);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteInt16(Int16 data)
        {
            Internal.WriteDataSInt(Bms1Tag.Int16, data); // writes attributes previously
        }

        public void WriteInt16(Int16? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataSInt(Bms1Tag.Int16, data.Value);
            }
        }

        public void WriteInt16Array(IEnumerable<Int16> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.Int16, data.Count() * 2);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteInt32(int data)
        {
            Internal.WriteDataSInt(Bms1Tag.Int32, data); // writes attributes previously
        }

        public void WriteInt32(Int32? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataSInt(Bms1Tag.Int32, data.Value);
            }
        }

        public void WriteInt32Array(IEnumerable<int> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.Int32, data.Count() * 4);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteInt64(Int64 data)
        {
            Internal.WriteDataSInt64(Bms1Tag.Int64, data);
        }

        public void WriteInt64(Int64? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteDataSInt64(Bms1Tag.Int64, data.Value);
            }
        }

        public void WriteInt64Array(IEnumerable<Int64> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.WriteAttributesAndTag(Bms1Tag.Int64, data.Count() * 8);
                foreach (var d in data)
                {
                    Stream.Write(d);
                }
            }
        }

        //------------------------------
        public void WriteEnum(Enum data)
        {
            Internal.WriteDataSInt64(Bms1Tag.Enum, (data as IConvertible).ToInt64(CultureInfo.InvariantCulture));
        }

        //------------------------------
        public void WriteUnicode(char data)
        {
            Internal.WriteAttributesAndTag(Bms1Tag.String + Bms1LengthSpec.L2);
            Stream.Write((Int16)data);
        }

        public void WriteUnicode(char? data)
        {
            if (!data.HasValue)
            {
                WriteNull();
            }
            else
            {
                WriteUnicode(data.Value);
            }
        }

        //------------------------------
        public void WriteString(string data)
        {
            Internal.WriteDataString(Bms1Tag.String, data); // supports null
        }

        public void WriteStrings(IList<string> data)
        {
            if (data == null)
            {
                WriteNull();
            }
            else
            {
                Internal.CollectionElementCount = data.Count;
                Internal.WriteAttributesAndTag(Bms1Tag.BlockStart);
                foreach (var s in data)
                {
                    Internal.WriteDataString(Bms1Tag.String, s); // supports null
                }
                Internal.WriteAttributesAndTag(Bms1Tag.BlockEnd);
            }
        }
    }
}
