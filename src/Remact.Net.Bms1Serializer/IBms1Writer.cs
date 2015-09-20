namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;
    
    public interface IBms1Writer
    {
        IBms1InternalWriter Internal { get; }
        
        
//        void WriteBlock(IBms1Dto blockDto);
//        
//        void WriteBlocks(IList<IBms1Dto> data);
//
//        void WriteBool(bool data);
        
        void WriteByte(byte data);

//        void WriteByteArray(byte[] data);
//        
//        void WriteInt(int data);
//        
//        void WriteChar(char data);
//        
//        void WriteString(string data);
//        
//        void WriteStrings(IList<string> data);
//        
//        void WriteDate(DateTime data);
//        void WriteTime(DateTime data);
//        void WriteDateTime(DateTime data);

    }
}
