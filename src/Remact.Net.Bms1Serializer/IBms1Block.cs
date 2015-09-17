namespace Remact.Net.Bms1Serializer
{
    public interface IBms1Block
    {
        void Bms1Read (Bms1Reader stream, int blockTypeId, Bms1Attributes attributes);
        void Bms1Write(Bms1Writer stream);
    }
}
