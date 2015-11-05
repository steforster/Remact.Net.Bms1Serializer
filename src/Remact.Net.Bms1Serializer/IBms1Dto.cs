using System;

namespace Remact.Net.Bms1Serializer
{
    public interface IBms1Dto
    {
        UInt16 Bms1BlockTypeId { get; }

        //void ReadFromBms1Stream(IBms1Reader reader);

        void WriteToBms1Stream(IBms1Writer writer);
    }
}
