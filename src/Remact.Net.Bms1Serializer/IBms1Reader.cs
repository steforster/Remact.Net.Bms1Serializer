namespace Remact.Net.Bms1Serializer
{
    public interface IBms1Reader
    {
        IBms1InternalReader Internal { get; }
        
        // returns next block type id
        int ReadBlockStart();
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool ReadBlock(IBms1Dto blockDto);
        

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadBool(ref bool data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadByte(ref byte data);

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadByteArray(ref byte[] data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadInt(ref int data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadString(ref string data);
        
        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        bool ReadChar(ref char data);
        
    }
}
