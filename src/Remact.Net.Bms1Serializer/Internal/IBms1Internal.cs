namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IBms1InternalReader
    {
        bool    EndOfBlock { get; } // before block start or after block- or message end.
        bool    IsCollection { get; } // attribute defines that a collection of elements of this type will follow. Each element has its own attributes and DataLength.
        int     CollectionElementCount { get; }  // -1 = no collection, -2 = collection until end of block (not a predefined length)

        Bms1Tag TagEnum { get; }
        bool    IsSingleValueOfType(Bms1Tag tag); // !EndOfBlock && TagEnum==tag && !IsArrayData

        bool    IsCharacterType { get; }
        int     BlockTypeId  { get; } // -1 = no id
        int     BlockNestingLevel { get; } // 1 = base block, 0 = message

        string       ObjectType { get; }    // null = no type
        string       ObjectName { get; }    // null = no name
        List<KeyValuePair<string, string>> KeyValueAttributes { get; }
        List<KeyValuePair<string, string>> NamespaceAttributes { get; }

        int     DataLength { get; } // length of following data in bytes, -2 = zero terminated, 0 = zero or empty array
        bool    IsArrayData { get; } // data is e.g. an array of bytes, ints...

        void    ReadAttributes(); // Reads attributes for next value. Does not read over end of block. 
        void    SkipData();

        // Does not read attribute and tag, reads over end of block.
        string  ReadDataString();
        uint    ReadDataUInt();
        Bms1Exception Bms1Exception(string message);
    }


    internal interface IMessageReader
    {
        // returns attributes of next message block
        IBms1InternalReader ReadMessageStart(BinaryReader binaryReader);

        // returns null (default(T)), when not read because: readMessageDto==null (message is skipped)
        T ReadMessage<T>(IBms1Reader reader, Func<IBms1Reader, T> readMessageDto) where T : new();

        // returns null (default(T)), when not read because: EndOfBlock, EndOfMessage, readDto==null (block is skipped)
        T ReadBlock<T>(Func<T, T> readDto) where T : new();
    }
    
    
    
    public interface IBms1InternalWriter
    {
        int  CollectionElementCount { get; set; }  // -1 = no collection, -2 = collection until end of block (not a predefined length)
        bool IsCharacterType { get; set; }
        //int  BlockTypeId { get; set; } // -1 = no id

        string       ObjectType { get; set; }    // null = no type
        string       ObjectName { get; set; }    // null = no name
        List<string> NameValueAttributes { get; set; }
        List<string> NamespaceAttributes { get; set; }

        void WriteAttributesAndTag(Bms1Tag tag); // Does not add a length specifier to the tag
        void WriteAttributesAndTag(Bms1Tag tag, int dataLength); // Adds length specifier and length according to data length: >= 256: writes 2 bytes data length / >= 0: writes 1 byte data length / -2 = zero terminated (no data length)
        
        // Does not write attributes:
        void WriteDataString(Bms1Tag tag, string data);
        void WriteDataUInt  (Bms1Tag tag, UInt32 data);
        void WriteDataUInt64(Bms1Tag tag, UInt64 data);
        void WriteDataSInt  (Bms1Tag tag, Int32  data);
        void WriteDataSInt64(Bms1Tag tag, Int64  data);
    }


    internal interface IMessageWriter
    {
        void WriteMessage(Bms1Writer writer, Action<IBms1Writer> writeDtoAction);
        void WriteBlock(IBms1Writer writer, int blockTypeId, Action writeDtoAction);
    }
}
