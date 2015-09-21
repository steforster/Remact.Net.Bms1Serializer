namespace Remact.Net.Bms1Serializer.Internal
{
    public enum Bms1Tag
    {
        // Known value tags.
        BoolFalse = 10,
        BoolTrue = 11,
        Null = 12,
        UByte = 20,  //  8 bit
        UInt16 = 30,  // 16 bit
        SInt16 = 40,
        UInt32 = 50,  // 32 bit
        SInt32 = 60,
        SInt64 = 70,  // 64 bit
        Enum = 80,  //  8...64 bit
        Bitset = 90,  //  8..128 bit
        Decimal = 100, //128 bit
        Float = 110, // 32 bit
        Double = 120, // 64 bit
        Date = 130,
        Time = 140,
        Char = 150, // all 10' tags must be defined up to 150, see 'TagReader' !

        // Known framing tags.
        NullBlock = 240,
        BaseBlockDefinition = 241,
        BlockStart = 242,
        BlockEnd = 244,
        MessageStart = 250,
        MessageFooter = 253,
        MessageEnd = 254,
        //Invalid = 255,

        Attribute = 256, // Any known or unknown attribute --> skip unknown attributes
        UnknownValue = 257  // Any unknown value tag --> skip only at end of block
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
