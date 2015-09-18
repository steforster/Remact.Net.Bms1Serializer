namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections;

    public interface IBms1InternalReader
    {
        // returns false, when EndOfBlock or EndOfMessage == true before or after reading next tag
        bool    ReadAttributes();
        
        bool    IsCollection { get; } // attribute defines that a collection of elements of this type will follow. Each element has its own attributes and DataLength.
        int     CollectionElementCount { get; }  // -1 = no collection

        Bms1Tag TypeTag { get; }
        
        bool    IsCharacterType { get; }
        bool    IsBlockType { get; }
        int     BlockTypeId  { get; } // -1 = no id
        int     BlockNestingLevel { get; } // 1 = base block, 0 = message

        string  ObjectType { get; }    // null = no type
        string  ObjectName { get; }    // null = no name
        IList   NameValueAttributes { get; }
        IList   NamespaceAttributes { get; }

        bool    EndOfBlock { get; } // before block start or after block end
        bool    EndOfMessage { get; } // before message start or after message end

        int     DataLength { get; } // length of following data in bytes, -1 = zero terminated
        bool    IsArrayData { get; } // data is e.g. an array of bytes

        void    SkipData();
        string  ReadDataString();
        uint    ReadDataUInt();
    }


    internal interface IBms1MessageReader
    {
        // returns next message block type
        int     ReadMessageStart();

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool    ReadMessage(Action dtoAction);

        // returns next block type
        int     ReadBlockStart();

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        bool    ReadBlock(Action dtoAction);
    }
}
