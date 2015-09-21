namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    internal class InternalReader : IBms1InternalReader, IMessageReader
    {
        internal BinaryReader Stream;
        private Attributes _attributes;
        private TagReader _tagReader;

        public InternalReader()
        {
            _attributes = new Attributes();
            _tagReader = new TagReader();
        }

        // returns false, when EndOfBlock or EndOfMessage == true after reading next tag
        private bool ReadNextTag()
        {
            while (true)
            {
                _attributes.Clear();
                IsBlockType = false;
                _attributes.ReadUntilNextValueOrFrameTag(Stream, _tagReader);

                if (_attributes.TagSetNumber != 0)
                {
                    _tagReader.TagEnum = Bms1Tag.UnknownValue;
                }

                // block or value tag and its attributes read, data is available for read
                if (_tagReader.TagEnum == Bms1Tag.MessageFooter || _tagReader.TagEnum == Bms1Tag.MessageEnd)
                {
                    EndOfMessage = true;
                    EndOfBlock = true;
                    if (BlockNestingLevel != 0)
                    {
                        BlockNestingLevel = 0;
                        throw new Bms1Exception("wrong block nesting at end of message: " + BlockNestingLevel);
                    }
                }

                if (!EndOfMessage)
                {
                    if (_tagReader.TagEnum == Bms1Tag.BlockStart)
                    {
                        EndOfBlock = false;
                        IsBlockType = true;
                        BlockNestingLevel++;
                    }

                    if (_tagReader.TagEnum == Bms1Tag.BlockEnd)
                    {
                        EndOfBlock = true;
                        BlockNestingLevel--;
                    }
                }

                return !EndOfBlock;
            }
        }


        #region IBms1MessageReader Members
        
        // returns next message block type
        public int ReadMessageStart(BinaryReader binaryReader)
        {
            Stream = binaryReader;
            while (true)
            {
//                try
//                {
                    EndOfMessage = true;
                    EndOfBlock = true;
                    BlockNestingLevel = 0;
                    if (_tagReader.TagEnum != Bms1Tag.MessageStart)
                    {
                        ReadNextTag();
                    }

                    if (_tagReader.TagEnum == Bms1Tag.MessageStart)
                    {
                        if (ReadNextTag() && _tagReader.TagEnum == Bms1Tag.BlockStart)
                        {
                            return _tagReader.BlockTypeId; // valid message- and block start
                        }
                    }
                    // not a valid message- and block start found
                    _tagReader.SkipData(Stream);
//                }
//                catch (Bms1Exception ex)
//                {
//                    // Invalid data or resynchronizing
//                }
            }// while
        }

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadMessage(Action dtoAction)
        {
            if (EndOfBlock || _tagReader.TagEnum != Bms1Tag.BlockStart || BlockNestingLevel != 1)
            {
                throw new Bms1Exception("not ready for message");
            }

            var ok = ReadBlock(dtoAction);

            while (_tagReader.TagEnum != Bms1Tag.MessageEnd && _tagReader.TagEnum != Bms1Tag.MessageStart)
            {
                // unknown blocks or values at end of message or resynchronization
                ReadNextTag();
                _tagReader.SkipData(Stream);
            }
            return ok;
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadBlock(Action dtoAction)
        {
            if (EndOfBlock)
            {
                return false;
            }

            var thisBlockLevel = BlockNestingLevel;
            Bms1Exception exception = null;
            bool ok = true;
            try
            {
                if (_tagReader.TagEnum != Bms1Tag.BlockStart)
                {
                    throw new Bms1Exception("wrong block start");
                }
                
                dtoAction(); // call the user code to deserialize the data transfer object

                if (BlockNestingLevel != thisBlockLevel)
                {
                    BlockNestingLevel = 0;
                    throw new Bms1Exception("wrong block nesting = " + BlockNestingLevel + " at end of block level: " + thisBlockLevel);
                }
            }
            catch (Bms1Exception ex)
            {
                exception = ex;
            }
            
            while (!BlockFinished (thisBlockLevel))
            {
                // skip unknown blocks or values at end of block or resynchronize
                ReadNextTag();
                _tagReader.SkipData(Stream);
            }

            if (BlockNestingLevel != thisBlockLevel - 1)
            {
                BlockNestingLevel = 0;
                throw new Bms1Exception("wrong block nesting = " + BlockNestingLevel + " after block level: " + thisBlockLevel, exception);
            }
            
            if (_tagReader.TagEnum != Bms1Tag.BlockEnd)
            {
                throw new Bms1Exception("no correct block end after block level: " + thisBlockLevel, exception);
            }
            ReadNextTag(); // get attributes of next value
            
            if (exception != null)
            {
                throw new Bms1Exception ("cannot read block level: " + thisBlockLevel, exception);
            }
            return ok;
        }
        
        private bool BlockFinished(int blockLevel)
        {
            if (_tagReader.TagEnum == Bms1Tag.MessageEnd || _tagReader.TagEnum == Bms1Tag.MessageStart)
            {
                return true;
            }
            return BlockNestingLevel <= blockLevel && (_tagReader.TagEnum == Bms1Tag.BlockEnd || _tagReader.TagEnum == Bms1Tag.BlockStart);
        }


        #endregion
        #region IBms1InternalReader Members
        

        public void ReadAttributes()
        {
            if (!EndOfBlock)
            {
                ReadNextTag();
            }
        }
        
        public bool IsCollection
        {
            get { return _attributes.CollectionElementCount >= 0; }
        }

        public int CollectionElementCount
        {
            get { return _attributes.CollectionElementCount; }
        }

        public Bms1Tag TagEnum
        {
            get { return _tagReader.TagEnum; }
        }

        public bool IsCharacterType
        {
            get { return _attributes.IsCharacterType; }
        }

        public bool IsBlockType
        {
            get; private set;
        }

        public int BlockTypeId
        {
            get { return IsBlockType ? _tagReader.BlockTypeId : -1; }
        }

        public int BlockNestingLevel
        {
            get; private set;
        }

        public string ObjectType
        {
            get { return _attributes.ObjectType; }
        }
        
        public string ObjectName
        {
            get { return _attributes.ObjectName; }
        }

        public List<string> NameValueAttributes
        {
            get { return _attributes.NameValue; }
        }

        public List<string> NamespaceAttributes
        {
            get { return _attributes.Namespace; }
        }

        public bool EndOfMessage
        {
            get; private set;
        }

        public bool EndOfBlock
        {
            get; private set;
        }

        public bool IsArrayData
        {
            get { return _tagReader.IsArrayData; }
        }

        public int DataLength
        {
            get { return _tagReader.DataLength; }
        }

        public void SkipData()
        {
            _tagReader.SkipData(Stream);
        }

        public string ReadDataString()
        {
            return _tagReader.ReadDataString(Stream);
        }

        public uint ReadDataUInt()
        {
            return _tagReader.ReadDataUInt(Stream);
        }

        public bool ThrowError(string message)
        {
            var attr = string.Empty;
            if (_attributes.CollectionElementCount >= 0)
            {
                attr = "Collection ["+_attributes.CollectionElementCount+"]";
            }
            if (_tagReader.IsArrayData) // attribute coded in the LengthSpecifier
            {
                attr += " Array of";
            }
            if (_attributes.IsCharacterType)
            {
                attr += " Char";
            }
            throw new Bms1Exception(string.Format("{0}. '{3}' TagByte={1}, DataLength={2}, ", message, attr, _tagReader.TagByte, _tagReader.DataLength));
        }
        
        #endregion
    }
}
