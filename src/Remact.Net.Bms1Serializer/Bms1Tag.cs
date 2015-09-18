namespace Remact.Net.Bms1Serializer
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
}
