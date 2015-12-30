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
            ClearAttributes();
        }

        #region IBms1MessageWriter Members


        public void WriteMessage(Bms1Writer writer, Action<IBms1Writer> writeDtoAction)
        {
            Stream = writer.Stream;
            Stream.Write((byte)Bms1Tag.MessageStart);
            Stream.Write((uint)0x544D4201);

            writeDtoAction(writer);
            
            Stream.Write((byte)Bms1Tag.MessageEnd);
        }


        public void WriteBlock(IBms1Writer writer, int blockTypeId, Action writeDtoAction)
        {
            WriteAttributes();
            if (blockTypeId < 0)
            {
                Stream.Write((byte)Bms1Tag.BlockStart);
            }
            else
            {
                Stream.Write((byte)(Bms1Tag.BlockStart + 1)); // 247
                Stream.Write((UInt16)blockTypeId);
            }

            writeDtoAction();
            
            Stream.Write((byte)Bms1Tag.BlockEnd);
            ClearAttributes();
        }


        private void ClearAttributes()
        {
            ObjectName = null;
            ObjectType = null;
            CollectionElementCount = Bms1Length.None; // -1
            NameValueAttributes = null;
            NamespaceAttributes = null;
            IsCharacterType = false;
            //BlockTypeId = -1;
        }

        private void WriteAttributes()
        {
            if (IsCharacterType)
            {
                Stream.Write((byte)Bms1Attribute.CharType);
                IsCharacterType = false;
            }

            if (CollectionElementCount >= 0)
            {
                WriteTagAndUInt((byte)Bms1Attribute.Collection, (UInt32)CollectionElementCount);
                CollectionElementCount = Bms1Length.None;
            }

            if (ObjectName != null)
            {
                WriteTagAndString((byte)Bms1Attribute.BlockName, ObjectName);
                ObjectName = null;
            }

            if (ObjectType != null)
            {
                WriteTagAndString((byte)Bms1Attribute.BlockType, ObjectType);
                ObjectType = null;
            }

            if (NameValueAttributes != null)
            {
                foreach (var item in NameValueAttributes)
                {
                    WriteTagAndString((byte)Bms1Attribute.NameValue, item);
                }
                NameValueAttributes = null;
            }

            if (NamespaceAttributes != null)
            {
                foreach (var item in NamespaceAttributes)
                {
                    WriteTagAndString((byte)Bms1Attribute.Namespace, item);
                }
                NamespaceAttributes = null;
            }
        }


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

        public void AddValueAttribute(string name, string value)
        {
            if (NameValueAttributes == null)
            {
                NameValueAttributes = new List<string>();
            }
            NameValueAttributes.Add(string.Concat(name, '=', value));
        }

        public void AddNamespaceAttribute(string alias, string fullNamespace)
        {
            if (NamespaceAttributes == null)
            {
                NamespaceAttributes = new List<string>();
            }
            NamespaceAttributes.Add(string.Concat(alias, '=', fullNamespace));
        }

        public void WriteAttributesAndTag(Bms1Tag tag)
        {
            WriteAttributes();
            Stream.Write((byte)tag);
        }

        public void WriteAttributesAndTag(Bms1Tag tag, int dataLength)
        {
            WriteAttributes();
            WriteTagAndLength((byte)tag, dataLength);
        }
        
        private void WriteTagAndLength(byte tag, int dataLength)
        {
            if (dataLength >= 256)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.Int32));
                Stream.Write((Int32)(dataLength));
            }
            else if (dataLength >= 0)
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.Byte));
                Stream.Write((byte)(dataLength));
            }
            else if (dataLength == Bms1Length.Open) // -2 = zero termination
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.ZeroTerminated)); 
            }
            else
            {
                throw new Bms1Exception("undefined dataLength=" + dataLength);
            }
        }

        private void WriteTagAndString(byte tag, string data)
        {
            if (data == null)
            {
                Stream.Write((byte)Bms1Tag.Null);
            }
            else if (data.Length == 0)
            {
                Stream.Write(tag);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                WriteTagAndLength(tag, buffer.Length);
                Stream.Write(buffer);
            }
        }

        private void WriteTagAndUInt(byte tag, UInt32 data)
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

        public void WriteDataString(Bms1Tag tag, string data)
        {
            WriteAttributes();
            WriteTagAndString((byte)tag, data);
        }

        public void WriteDataUInt(Bms1Tag tag, UInt32 data)
        {
            WriteAttributes();
            WriteTagAndUInt((byte)tag, data);
        }

        public void WriteDataUInt64(Bms1Tag tag, UInt64 data)
        {
            WriteAttributes();
            if (data <= 0xFFFFFFFF)
            {
                WriteTagAndUInt((byte)tag, (UInt32)data);
            }
            else
            {
                Stream.Write((byte)(tag + Bms1LengthSpec.L8));
                Stream.Write(data);
            }
        }

        public void WriteDataSInt(Bms1Tag tag, Int32 data)
        {
            WriteAttributes();
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
                    WriteAttributes();
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
                    WriteAttributes();
                    Stream.Write((byte)(tag + Bms1LengthSpec.L8));
                    Stream.Write(data);
                }
            }
        }

        #endregion
    }
}
