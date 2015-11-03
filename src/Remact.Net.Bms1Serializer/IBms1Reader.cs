namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;
    
    public interface IBms1Reader
    {
        IBms1InternalReader Internal { get; }
        
        // ??? returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool ReadBlock(IBms1Dto blockDto);
        
        bool ReadBlocks(Func<IBms1InternalReader, IBms1Dto> blockFactory);

        // check Internal.EndOfBlock before calling Read... method.
        bool ReadBool();
        byte ReadByte();
        UInt16 ReadUInt16();
        UInt32 ReadUInt32();
        Int16 ReadInt16();
        Int32 ReadInt32();
        Int64 ReadInt64();

        Nullable<T> ReadNullable<T>() where T : struct;

        byte[] ReadByteArray();
        UInt16[] ReadUInt16Array();
        UInt32[] ReadUInt32Array();
        Int16[] ReadInt16Array();
        Int32[] ReadInt32Array();
        Int64[] ReadInt64Array();

        char  ReadChar();
        string ReadString();
        IList<string> ReadStrings();
//        
//        bool ReadDate(ref DateTime data);
//        bool ReadTime(ref DateTime data);
//        bool ReadDateTime(ref DateTime data);
    }
}
