namespace Remact.Net.Bms1Serializer
{
    public interface IBms1Dto
    {
        void Bms1Read (IBms1Reader reader, int blockTypeId);
        void Bms1Write(Bms1Writer stream);
    }
}
