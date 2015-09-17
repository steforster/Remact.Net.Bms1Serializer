namespace Remact.Net.Bms1Serializer
{
    using System.IO;
    using System.Text;

    public enum Bms1Attr
    {
//        AlternateTagSet1, // 0
//        AlternateTagSet2,
//        AlternateTagSet3,
        CharacterType,
//        Undef17,
//        Undef18,
//        Undef19,
//        Undef160, // 7
        Name,
        Type,
        Array,
        NameValue,
        Namespace,
//        Undef220,
//        Undef230 //14
    }

    public class Bms1Attributes
    {
        private int _attributes;

        public string ObjectName   { get; private set; }    // null = no name
        public string ObjectType   { get; private set; }
        public uint   ArrayLength  { get; private set; }    // 0 = no array
        public string NameValue    { get; private set; }    // TODO: array
        public string Namespace    { get; private set; }    // TODO: array
        public int    TagSetNumber { get; private set; }    // only last attribute is used, no combinations supported

        internal void Clear()
        {
            _attributes = 0;
            ObjectName = null;
            ObjectType = null;
            ArrayLength = 0;
            NameValue = null;
            Namespace = null;
            TagSetNumber = 0;
        }

        private void Set(Bms1Attr attr)
        {
            _attributes |= 1 << (int)attr;
        }

        public bool IsSet(Bms1Attr attr)
        {
            int mask = 1 << (int)attr;
            return (_attributes & mask) == mask;
        }

        internal void ReadUntilNextValueOrFrameTag(BinaryReader stream, Bms1Tag tag)
        {
            while (true)
            {
                tag.ReadTag(stream);
                if (tag.Type != Tag.Attribute)
                {
                    // known value- or framing tag found
                    return;
                }

                // attribute is read, data is available for read
                switch (tag.AttributeTagType)
                {
                    case  13: TagSetNumber = 1; break;
                    case  14: TagSetNumber = 2; break;
                    case  15: TagSetNumber = 3; break;
                    
                    case  16: Set(Bms1Attr.CharacterType); break;

                    case 170: Set(Bms1Attr.Name);
                        ObjectName = tag.ReadDataString(stream);
                        break;

                    case 180: Set(Bms1Attr.Type);
                        ObjectType = tag.ReadDataString(stream);
                        break;

                    case 190: Set(Bms1Attr.Array);
                        ArrayLength = tag.ReadDataUInt(stream);
                        break;

                    case 200: Set(Bms1Attr.NameValue);
                        NameValue = tag.ReadDataString(stream);
                        break;

                    case 210: Set(Bms1Attr.Namespace);
                        Namespace = tag.ReadDataString(stream);
                        break;

                    default:
                        tag.SkipData(stream);
                        break;
                }
            }
        }
    }
}
