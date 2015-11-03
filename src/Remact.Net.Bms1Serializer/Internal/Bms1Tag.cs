namespace Remact.Net.Bms1Serializer.Internal
{
    public enum Bms1Tag
    {
        // Known value tags.
        Null = 7,
        BoolFalse = 8,
        BoolTrue = 9,
        Byte = 10,  //  8 bit, unsigned
        UInt16 = 20,  // 16 bit, unsigned
        Int16 = 30,   // 16 bit, signed
        UInt32 = 40,  // 32 bit
        Int32 = 50,
        Int64 = 60,  // 64 bit, signed
        Enum = 70,  //  8...64 bit
        Bitset = 80,  //  8..128 bit
        Decimal = 90, //128 bit
        Float = 100, // 32 bit
        Double = 110, // 64 bit
        Date = 120,
        Time = 130,
        Char = 140, // all 10' tags must be defined up to 140, see 'TagReader' !

        // Known framing tags.
        MessageStart = 245,
        BlockStart = 246,
        NullBlock = 248,
        BlockEnd = 249,
        MessageFooter = 251,
        MessageEnd = 252,
        //Invalid = 255,

        Attribute = 256, // Any known or unknown attribute --> skip unknown attributes
        UnknownValue = 257  // Any unknown value tag --> skip only at end of block
    }

    internal enum Bms1Attribute
    {
        BlockName = 170,
        BlockType = 180,
        BaseBlockType = 182,
        NameValue = 190,
        Namespace = 200,
        Collection = 230,
        CharType = 240,
        TagSet1 = 241,
        TagSet2 = 242,
        TagSet3 = 243,
    }

    public class Bms1LengthSpec
    {
        public const int ZeroTerminated = 5;
        public const int Byte = 6;
        public const int Int32 = 7;
        public const int L0 = 0;
        public const int L1 = 1;
        public const int L2 = 2;
        public const int L4 = 4;
        public const int L8 = 8;
        public const int L16 = 9;
    }
}
