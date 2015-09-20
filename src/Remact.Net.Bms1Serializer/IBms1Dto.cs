namespace Remact.Net.Bms1Serializer
{
    public interface IBms1Dto
    {
        void Bms1Read (IBms1Reader reader);
        void Bms1Write(IBms1Writer writer);
    }
}
