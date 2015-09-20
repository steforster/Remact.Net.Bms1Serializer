namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IBms1InternalReader
    {
        bool    EndOfBlock { get; } // before block start or after block end, also not EndOfMessage
        bool    EndOfMessage { get; } // before message start or after message end

        // returns false, when EndOfBlock or EndOfMessage == true before or after reading next tag
        bool    IsCollection { get; } // attribute defines that a collection of elements of this type will follow. Each element has its own attributes and DataLength.
        int     CollectionElementCount { get; }  // -1 = no collection

        Bms1Tag TypeTag { get; }
        
        bool    IsCharacterType { get; }
        bool    IsBlockType { get; }
        int     BlockTypeId  { get; } // -1 = no id
        int     BlockNestingLevel { get; } // 1 = base block, 0 = message

        string  ObjectType { get; }    // null = no type
        string  ObjectName { get; }    // null = no name
        List<string>   NameValueAttributes { get; }
        List<string>   NamespaceAttributes { get; }

        int     DataLength { get; } // length of following data in bytes, -1 = zero terminated
        bool    IsArrayData { get; } // data is e.g. an array of bytes

        void    ReadAttributes(); // when not end of block or end of message, reads attributes for next value
        void    SkipData();
        string  ReadDataString();
        uint    ReadDataUInt();
        bool    ThrowError(string message);
    }


    internal interface IMessageReader
    {
        // returns next message block type
        int     ReadMessageStart(BinaryReader binaryReader);

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool    ReadMessage(Action dtoAction);

        // returns next block type
        //int     ReadBlockStart();

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool    ReadBlock(Action dtoAction);
    }
    
    
    
    public interface IBms1InternalWriter
    {
        int     CollectionElementCount { set; }  // -1 = no collection

        Bms1Tag TypeTag { set; }
        
        bool    IsCharacterType { set; }
        int     BlockTypeId  { set; } // -1 = no id

        string  ObjectType { set; }    // null = no type
        string  ObjectName { set; }    // null = no name
        List<string>   NameValueAttributes { set; }
        List<string>   NamespaceAttributes { set; }

        int     DataLength { set; } // length of following data in bytes, -1 = zero terminated
        bool    IsArrayData { set; } // data is e.g. an array of bytes

        void    WriteDataString(Bms1Tag tag, string data);
        void    WriteDataUInt(uint data);
    }


    internal interface IMessageWriter
    {
        void    WriteMessage(BinaryWriter binaryWriter, int blockTypeId, Action dtoAction);
        void    WriteBlock(int blockTypeId, Action dtoAction);
    }
}
