namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;
    
    public interface IBms1Reader
    {
        IBms1InternalReader Internal { get; }
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool ReadBlock(IBms1Dto blockDto);
        
        bool ReadBlocks(Func<IBms1InternalReader, IBms1Dto> blockFactory);

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadBool(ref bool data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadByte(ref byte data);

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadByteArray(ref byte[] data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadInt(ref int data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadChar(ref char data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadString(ref string data);
        
//        bool ReadStrings(ref IList<string> data);
//        
//        bool ReadDate(ref DateTime data);
//        bool ReadTime(ref DateTime data);
//        bool ReadDateTime(ref DateTime data);
    }
}
