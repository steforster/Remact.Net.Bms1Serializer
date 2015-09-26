namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;

    public interface IBms1Writer
    {
        IBms1InternalWriter Internal { get; }


        //        void WriteBlock(IBms1Dto blockDto);

        //        void WriteBlocks(IList<IBms1Dto> data);

        void WriteBool(bool data);
        void WriteBool(bool? data);

        void WriteByte(byte data);
        void WriteByteArray(byte[] data);
        void WriteByteArray(IEnumerable<Byte> data);
        void WriteByte(byte? data);

        void WriteUInt16(UInt16 data);
        void WriteUInt16Array(IEnumerable<UInt16> data);
        void WriteUInt16(UInt16? data);

        void WriteUInt32(UInt32 data);
        void WriteUInt32Array(IEnumerable<UInt32> data);
        void WriteUInt32(UInt32? data);

        void WriteInt16(Int16 data);
        void WriteInt16Array(IEnumerable<Int16> data);
        void WriteInt16(Int16? data);

        void WriteInt32(Int32 data);
        void WriteInt32Array(IEnumerable<Int32> data);
        void WriteInt32(Int32? data);

        void WriteInt64(Int64 data);
        void WriteInt64Array(IEnumerable<Int64> data);
        void WriteInt64(Int64? data);

        void WriteUnicode(char data);
        void WriteUnicode(char? data);

        void WriteString(string data);
        //        
        //        void WriteStrings(IList<string> data);
        //        
        //        void WriteDate(DateTime data);
        //        void WriteTime(DateTime data);
        //        void WriteDateTime(DateTime data);

    }
}
