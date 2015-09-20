namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    internal class InternalWriter : IBms1InternalWriter, IMessageWriter
    {
        internal BinaryWriter Stream;
        

        public InternalWriter()
        {
        }

        #region IBms1MessageWriter Members
        

        public void WriteMessage(BinaryWriter binaryWriter, int blockTypeId, Action dtoAction)
        {
            Stream = binaryWriter;
            Stream.Write((byte)Bms1Tag.MessageStart);
            Stream.Write((uint)0x544D4201);
            
            WriteBlock(blockTypeId, dtoAction);
            
            Stream.Write((byte)Bms1Tag.MessageEnd);
        }


        public void WriteBlock(int blockTypeId, Action dtoAction)
        {
            WriteAttributes();
            if (blockTypeId < 0)
            {
                Stream.Write((byte)Bms1Tag.BlockStart);
            }
            else
            {
                Stream.Write((byte)(Bms1Tag.BlockStart + 1));
                Stream.Write((UInt16)blockTypeId);
            }
            
            dtoAction();
            
            Stream.Write((byte)Bms1Tag.BlockEnd);
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
                Stream.Write((byte)16);
            }

            if (CollectionElementCount >= 0)
            {
                Stream.Write((byte)190);
                Stream.Write(CollectionElementCount);
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

        public void WriteDataLength(Bms1Tag tag, int dataLength)
        {
            if (dataLength < 0)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.ZeroTerminated));
            }
            else if (dataLength <= 255)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.Byte));
                Stream.Write((byte)(dataLength));
            }
            else
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.Int32));
                Stream.Write((Int32)(dataLength));
            }
        }
        
        public void WriteDataString(Bms1Tag tag, string data)
        {
            if (data.Length == 0)
            {
                Stream.Write((byte)tag);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                WriteDataLength(tag, buffer.Length);
                Stream.Write(buffer);
            }
        }

        public void WriteDataUInt(Bms1Tag tag, UInt32 data)
        {
            if (data == 0)
            {
                Stream.Write((byte)tag);
            }
            else if (data <= 0xFF)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.L1));
                Stream.Write((byte)(data));
            }
            else if (data <= 0xFFFF)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.L2));
                Stream.Write((UInt16)(data));
            }
            else
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.L4));
                Stream.Write(data);
            }
        }

        public void WriteDataUInt64(Bms1Tag tag, UInt64 data)
        {
            if (data <= 0xFFFFFFFF)
            {
                WriteDataUInt(tag, (UInt32)data);
            }
            else
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.L8));
                Stream.Write(data);
            }
        }

        public void WriteDataSInt(Bms1Tag tag, Int32 data)
        {
            if (data == 0)
            {
                Stream.Write((byte)tag);
            }
            else if (data > 0)
            {
                // Positive, most significant bit must be zero
                if (data < 0x80)
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L1));
                    Stream.Write((SByte)(data));
                }
                else if (data < 0x8000)
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L2));
                    Stream.Write((Int16)(data));
                }
                else
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L4));
                    Stream.Write(data);
                }
            }
            else
            {   // Negative, most significant bit must be set
                if (data >= -0x80)// > 0xFFFFFF70)
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L1));
                    Stream.Write((SByte)(data));
                }
                else if (data >= -0x8000) // > 0xFFFF7000)
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L2));
                    Stream.Write((Int16)(data));
                }
                else
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L4));
                    Stream.Write(data);
                }
            }
        }

        public void WriteDataSInt64(Bms1Tag tag, Int64 data)
        {
            if (data >= 0)
            {
                if (data < 0x80000000)
                {
                    WriteDataSInt(tag, (Int32)data);
                }
                else
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L8));
                    Stream.Write(data);
                }
            }
            else
            {
                if (data >= -0x80000000)
                {
                    WriteDataSInt(tag, (Int32)data);
                }
                else
                {
                    Stream.Write((byte)(tag + Bms1LengthSpec.L8));
                    Stream.Write(data);
                }
            }
        }

        #endregion
    }
}
