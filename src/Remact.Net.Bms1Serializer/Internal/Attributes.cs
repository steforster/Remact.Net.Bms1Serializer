﻿namespace Remact.Net.Bms1Serializer.Internal
{
    using System.IO;
    using System.Text;


    internal class Attributes
    {
        public string ObjectName;   // null = no name

        public string ObjectType;

        public int CollectionElementCount;    // -1 = no collection

        public string NameValue;    // TODO: list

        public string Namespace;    // TODO: list

        public int TagSetNumber;    // only last attribute is used, no combinations supported

        public bool IsCharacterType;


        internal void Clear()
        {
            ObjectName = null;
            ObjectType = null;
            CollectionElementCount = -1;
            NameValue = null;
            Namespace = null;
            TagSetNumber = 0;
            IsCharacterType = false;
        }

        internal void ReadUntilNextValueOrFrameTag(BinaryReader stream, TagReader tag)
        {
            while (true)
            {
                tag.ReadTag(stream);
                if (tag.TypeTag != Bms1Tag.Attribute)
                {
                    // known value- or framing tag found
                    return;
                }

                // attribute is read, data is available for read
                switch (tag.AttributeTag)
                {
                    case  13: TagSetNumber = 1; break;
                    case  14: TagSetNumber = 2; break;
                    case  15: TagSetNumber = 3; break;

                    case  16: IsCharacterType = true; break;

                    case 170: ObjectName = tag.ReadDataString(stream);
                        break;

                    case 180: ObjectType = tag.ReadDataString(stream);
                        break;

                    case 190: CollectionElementCount = (int)tag.ReadDataUInt(stream);
                        if (CollectionElementCount < 0) // TODO Max
                        {
                            throw new Bms1Exception("array length out of bounds: " + CollectionElementCount);
                        }
                        break;

                    case 200: NameValue = tag.ReadDataString(stream);
                        break;

                    case 210: Namespace = tag.ReadDataString(stream);
                        break;

                    default: tag.SkipData(stream);
                        break;
                }
            }
        }
    }
}
