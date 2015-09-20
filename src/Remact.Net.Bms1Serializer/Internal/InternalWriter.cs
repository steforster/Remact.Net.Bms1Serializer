namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    internal class InternalWriter : IBms1InternalWriter, IMessageWriter
    {
        private BinaryWriter _stream;
        

        public InternalWriter()
        {
        }

        #region IBms1MessageWriter Members
        

        public void WriteMessage(BinaryWriter binaryWriter, int blockTypeId, Action dtoAction)
        {
            _stream = binaryWriter;
            _stream.Write((byte)Bms1Tag.MessageStart);
            _stream.Write((uint)0x544D4201);
            
            WriteBlock(blockTypeId, dtoAction);
            
            _stream.Write((byte)Bms1Tag.MessageEnd);
        }


        public void WriteBlock(int blockTypeId, Action dtoAction)
        {
            WriteAttributes();
            if (blockTypeId < 0)
            {
                _stream.Write((byte)Bms1Tag.BlockStart);
            }
            else
            {
                _stream.Write((byte)(Bms1Tag.BlockStart + 1));
                _stream.Write((UInt16)blockTypeId);
            }
            
            dtoAction();
            
            _stream.Write((byte)Bms1Tag.BlockEnd);
            ClearAttributes();
        }

        private void ClearAttributes()
        {
            ObjectName = null;
            ObjectType = null;
            CollectionElementCount = -1;
            NameValueAttributes = null;
            NamespaceAttributes = null;
            IsCharacterType = false;
        }

        private void WriteAttributes()
        {
            if (IsCharacterType)
            {
                _stream.Write((byte)16);
            }

            if (CollectionElementCount >= 0)
            {
                _stream.Write((byte)190);
                _stream.Write(CollectionElementCount);
            }
        }

//        private void WriteTag(Bms1Tag tag, int lengthSpecifier)
//        {
//            int tagByte = lengthSpecifier + (int)tag;
//            _stream.Write((byte)tagByte);
//        }

        #endregion
        #region IBms1InternalWriter Members
        

        public int CollectionElementCount
        {
            get; set;
        }

        public Bms1Tag TypeTag
        {
            get; set;
        }

        public bool IsCharacterType
        {
            get; set;
        }

        public int BlockTypeId
        {
            get; set;
        }

        public string ObjectType
        {
            get; set;
        }
        
        public string ObjectName
        {
            get; set;
        }

        public List<string> NameValueAttributes
        {
            get; set;
        }

        public List<string> NamespaceAttributes
        {
            get; set;
        }

        public bool IsArrayData
        {
            get; set;
        }

        public int DataLength
        {
            get; set;
        }

        public void WriteDataString(Bms1Tag tag, string data)
        {
            if (data.Length == 0)
            {
                _stream.Write((byte)tag);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                if (buffer.Length < 256)
                {
                    _stream.Write((byte)(tag + Bms1LengthSpec.Byte));
                    _stream.Write((byte)buffer.Length);
                }
                else
                {
                    _stream.Write((byte)(tag + Bms1LengthSpec.Int32));
                    _stream.Write((Int32)buffer.Length);
                }
                _stream.Write(buffer);
            }
        }

        public void WriteDataUInt(uint data)
        {
        }

        #endregion
    }
}
