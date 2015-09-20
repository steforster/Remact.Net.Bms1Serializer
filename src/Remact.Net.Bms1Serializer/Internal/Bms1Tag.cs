namespace Remact.Net.Bms1Serializer.Internal
{
    public enum Bms1Tag
    {
        // Known value tags.
        BoolFalse = 10,
        BoolTrue = 11,
        Null = 12,
        UByte = 20,  //  8 bit
        UShort = 30,  // 16 bit
        SShort = 40,
        UInt = 50,  // 32 bit
        SInt = 60,
        SLong = 70,  // 64 bit
        Enum = 80,  //  8...64 bit
        Bitset = 90,  //  8..128 bit
        Decimal = 100, //128 bit
        Float = 110, // 32 bit
        Double = 120, // 64 bit
        Date = 130,
        Time = 140,
        String = 150,

        // Known framing tags.
        NullBlock = 240,
        BaseBlockDefinition = 241,
        BlockStart = 242,
        BlockEnd = 244,
        Undefined = 246,
        MessageStart = 250,
        MessageFooter = 253,
        MessageEnd = 254,
        //Invalid = 255,

        // Any attribute or unknown value tag --> allowed to skip
        Attribute = 256
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
