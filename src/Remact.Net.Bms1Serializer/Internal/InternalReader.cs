namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    internal class InternalReader : IBms1InternalReader, IMessageReader
    {
        internal BinaryReader Stream; // set by TestSerializer for testing purpose.
        private TagReader _tagReader;

        public InternalReader()
        {
            _tagReader = new TagReader();
        }

        // returns false, when EndOfBlock or EndOfMessage == true after reading next tag
        private bool ReadNextTag()
        {
            _tagReader.ClearAttributes();
            while (_tagReader.TagEnum == Bms1Tag.Attribute)
            {
                _tagReader.ReadTag(Stream);
            }

            if (_tagReader.TagSetNumber != 0)
            {
                _tagReader.TagEnum = Bms1Tag.UnknownValue;
            }

            // block or value tag and its attributes are read, data is available for read
            if (_tagReader.TagEnum == Bms1Tag.MessageFooter || _tagReader.TagEnum == Bms1Tag.MessageEnd)
            {
                EndOfMessage = true;
                if (BlockNestingLevel != 0)
                {
                    EndOfBlock = true;
                    BlockNestingLevel = 0;
                    throw new Bms1Exception("wrong block nesting at end of message: " + BlockNestingLevel);
                }
            }

            if (EndOfMessage)
            {
                EndOfBlock = true;
            }
            else
            {
                if (_tagReader.TagEnum == Bms1Tag.BlockEnd)
                {
                    BlockNestingLevel--;
                    EndOfBlock = true;
                }
                else
                {
                    if (_tagReader.TagEnum == Bms1Tag.BlockStart)
                    {
                        BlockNestingLevel++;
                    }
                    EndOfBlock = false;
                }
            }

            return !EndOfBlock;
        }


        #region IBms1MessageReader Members

        // returns attributes of next message block
        public IBms1InternalReader ReadMessageStart(BinaryReader binaryReader)
        {
            Stream = binaryReader;
            while (true)
            {
                EndOfMessage = false;
                EndOfBlock = false;
                BlockNestingLevel = 0;
                if (_tagReader.TagEnum != Bms1Tag.MessageStart)
                {
                    _tagReader.SkipData(Stream);
                    ReadNextTag();
                }

                if (_tagReader.TagEnum == Bms1Tag.MessageStart)
                {
                    if (ReadNextTag() && _tagReader.TagEnum == Bms1Tag.BlockStart)
                    {
                        return this; // valid message- and block start
                    }
                }
            }// while
        }

        // returns null (default(T)), when not read because: readMessageDto==null (message is skipped)
        public T ReadMessage<T>(IBms1Reader reader, Func<IBms1Reader, T> readMessageDto) where T : new()
        {
            if (EndOfBlock || _tagReader.TagEnum != Bms1Tag.BlockStart || BlockNestingLevel != 1)
            {
                throw new Bms1Exception("stream is not at start of message");
            }

            T dto = readMessageDto(reader);

            while (_tagReader.TagEnum != Bms1Tag.MessageEnd && _tagReader.TagEnum != Bms1Tag.MessageStart)
            {
                // unknown blocks or values at end of message or resynchronization
                _tagReader.SkipData(Stream);
                ReadNextTag();
            }

            return dto;
        }

        // returns null (default(T)), when not read because: EndOfBlock, EndOfMessage, readDto==null (block is skipped)
        public T ReadBlock<T>(Func<T,T> readDto) where T : new()
        {
            T dto = default(T); // null, when object
            if (EndOfBlock)
            {
                return dto;
            }

            var thisBlockLevel = BlockNestingLevel;
            Bms1Exception exception = null;
            try
            {
                if (_tagReader.TagEnum == Bms1Tag.Null)
                {
                    ReadNextTag(); // get attributes of next value
                    return dto; // null or default
                }

                if (_tagReader.TagEnum != Bms1Tag.BlockStart)
                {
                    throw new Bms1Exception("stream is not at start of block");
                }
                ReadNextTag(); // get attributes of first value

                if (readDto != null)
                {
                    dto = readDto(new T()); // create a default DTO and call the user code to deserialize the data transfer object
                }
            }
            finally
            {
                while (!BlockFinished(thisBlockLevel))
                {
                    // skip unknown blocks or values at end of block or resynchronize
                    _tagReader.SkipData(Stream);
                    ReadNextTag();
                }

                if (!EndOfMessage)
                {
                    var endOfBlockLevel = BlockNestingLevel + 1;
                    if (endOfBlockLevel != thisBlockLevel)
                    {
                        BlockNestingLevel = 0;
                        throw new Bms1Exception(
                            "wrong block nesting = " + endOfBlockLevel + " at end of block level: " + thisBlockLevel,
                            exception);
                    }

                    if (_tagReader.TagEnum != Bms1Tag.BlockEnd)
                    {
                        throw new Bms1Exception("no correct block end after block level: " + thisBlockLevel, exception);
                    }
                    ReadNextTag(); // get attributes of next value
                }
            }

            return dto;
        }
        
        private bool BlockFinished(int blockLevel)
        {
            if (_tagReader.TagEnum == Bms1Tag.MessageEnd || _tagReader.TagEnum == Bms1Tag.MessageStart)
            {
                EndOfMessage = true;
                return true;
            }
            return BlockNestingLevel < blockLevel && (_tagReader.TagEnum == Bms1Tag.BlockEnd || _tagReader.TagEnum == Bms1Tag.BlockStart);
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
            get { return _tagReader.CollectionElementCount != Bms1Length.None; }
        }

        // -1 = no collection, -2 = collection until end of block (not a predefined length)
        public int CollectionElementCount
        {
            get { return _tagReader.CollectionElementCount; }
        }

        public Bms1Tag TagEnum
        {
            get { return _tagReader.TagEnum; }
        }

        public bool IsSingleValueOfType(Bms1Tag tag)
        {
            return !EndOfBlock && _tagReader.TagEnum == tag && !_tagReader.IsArrayData;
        }

        public bool IsCharacterType
        {
            get { return _tagReader.IsCharacterType; }
        }

        public int BlockTypeId
        {
            get { return _tagReader.BlockTypeId; }
        }

        public int BlockNestingLevel
        {
            get; private set;
        }

        public string ObjectType
        {
            get { return _tagReader.ObjectType; }
        }
        
        public string ObjectName
        {
            get { return _tagReader.ObjectName; }
        }

        public List<string> NameValueAttributes
        {
            get { return _tagReader.NameValues; }
        }

        public List<string> NamespaceAttributes
        {
            get { return _tagReader.Namespaces; }
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

        public Bms1Exception Bms1Exception(string message)
        {
            var attr = string.Empty;

            if (EndOfBlock)
            {
                attr += "EndOfBlock";
            }

            if (IsCollection)
            {
                attr += " Collection["+ _tagReader.CollectionElementCount+"]";
            }

            if (IsArrayData) // attribute coded in the LengthSpecifier
            {
                attr += " Array";
            }

            if (IsCharacterType)
            {
                attr += " Char";
            }
            return new Bms1Exception(string.Format("{0} @ Type={1}, Tag={2}({3}.{4}), Len={5}, Attr='{6}'", message, _tagReader.BlockTypeId, _tagReader.TagEnum, _tagReader.TagByte, _tagReader.TagSetNumber, _tagReader.DataLength, attr));
        }
        
        #endregion
    }
}
