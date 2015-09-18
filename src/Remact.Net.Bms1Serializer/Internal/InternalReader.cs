namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;

    internal class InternalReader : IBms1InternalReader, IBms1MessageReader
    {
        private BinaryReader _stream;
        private Attributes _attributes;
        private TagReader _reader;

        public InternalReader(BinaryReader streamReader)
        {
            _stream = streamReader;
            _attributes = new Attributes();
            _reader = new TagReader();
        }

        // returns false, when EndOfBlock or EndOfMessage == true after reading next tag
        private bool ReadNextTag()
        {
            while (true)
            {
                _attributes.Clear();
                IsBlockType = false;
                _attributes.ReadUntilNextValueOrFrameTag(_stream, _reader);

                if (!IsSupported(_reader.TypeTag))
                {
                    _reader.SkipData(_stream);
                    continue; // try if next tag is known
                }

                // known tag and its attributes read, data is available for read
                if (_reader.TypeTag == Bms1Tag.MessageFooter || _reader.TypeTag == Bms1Tag.MessageEnd)
                {
                    if (BlockNestingLevel != 0)
                    {
                        BlockNestingLevel = 0;
                        throw new Bms1Exception("wrong block nesting at end of message: " + BlockNestingLevel);
                    }
                    EndOfMessage = true;
                    EndOfBlock = true;
                }

                if (!EndOfMessage)
                {
                    if (_reader.TypeTag == Bms1Tag.BlockStart)
                    {
                        EndOfBlock = false;
                        IsBlockType = true;
                        BlockNestingLevel++;
                    }

                    if (_reader.TypeTag == Bms1Tag.BlockEnd)
                    {
                        EndOfBlock = true;
                        BlockNestingLevel--;
                    }
                }

                return !EndOfMessage && !EndOfBlock;
            }
        }

        private bool IsSupported(Bms1Tag tagType)
        {
            if (_attributes.TagSetNumber != 0)
            {
                return false;
            }

            switch (tagType)
            {
                case Bms1Tag.BoolFalse:
                case Bms1Tag.BoolTrue:
                case Bms1Tag.UByte:
                case Bms1Tag.SInt: // TODO arrays
                case Bms1Tag.String:

                case Bms1Tag.MessageStart:
                case Bms1Tag.MessageFooter:
                case Bms1Tag.MessageEnd:
                case Bms1Tag.BlockStart:
                case Bms1Tag.BlockEnd:
                    return true;

                default:
                    return false;
            }
        }

        // returns next message block type
        public int ReadMessageStart()
        {
            while (true)
            {
                try
                {
                    EndOfMessage = true;
                    EndOfBlock = true;
                    BlockNestingLevel = 0;
                    if (_reader.TypeTag != Bms1Tag.MessageStart)
                    {
                        ReadNextTag();
                    }

                    if (_reader.TypeTag == Bms1Tag.MessageStart)
                    {
                        if (ReadNextTag() && _reader.TypeTag == Bms1Tag.BlockStart)
                        {
                            return _reader.BlockTypeId; // valid message- and block start
                        }
                    }
                    // not a valid message- and block start found
                    _reader.SkipData(_stream);
                }
                catch (Bms1Exception ex)
                {
                    // Invalid data or resynchronizing
                }
            }// while
        }

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadMessage(Action dtoAction)
        {
            if (EndOfMessage || EndOfBlock)
            {
                throw new Bms1Exception("not ready for message");
            }

            var ok = ReadBlock(dtoAction);

            while (_reader.TypeTag != Bms1Tag.MessageEnd && _reader.TypeTag != Bms1Tag.MessageStart)
            {
                // unknown blocks or values at end of message or resynchronization
                ReadNextTag();
                _reader.SkipData(_stream);
            }
            return ok;
        }

        // returns next block type
        public int ReadBlockStart()
        {
            if (EndOfMessage || !ReadNextTag() || _reader.TypeTag != Bms1Tag.BlockStart)
            {
                throw new Bms1Exception("wrong block start");
            }
            
            return _reader.BlockTypeId;
        }
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadBlock(Action dtoAction)
        {
            if (EndOfMessage)
            {
                return false;
            }

            if (_reader.TypeTag != Bms1Tag.BlockStart)
            {
                ReadNextTag();
            }

            if (EndOfBlock || _reader.TypeTag != Bms1Tag.BlockStart)
            {
                throw new Bms1Exception("wrong block start");
            }

            var thisBlockLevel = BlockNestingLevel;
            
            bool ok = true;
            try
            {
                dtoAction(); // call the user code to deserialize the data transfer object

                if (BlockNestingLevel != thisBlockLevel)
                {
                    BlockNestingLevel = 0;
                    throw new Bms1Exception("wrong block nesting = " + BlockNestingLevel + " at end of block level: " + thisBlockLevel);
                }
            }
            catch (Bms1Exception ex)
            {
                ok = false;
            }
            
            while (!BlockFinished (thisBlockLevel))
            {
                // skip unknown blocks or values at end of block or resynchronization
                ReadNextTag();
                _reader.SkipData(_stream);
            }

            if (BlockNestingLevel != thisBlockLevel - 1)
            {
                BlockNestingLevel = 0;
                throw new Bms1Exception("wrong block nesting = " + BlockNestingLevel + " at after block level: " + thisBlockLevel);
            }
            return ok;
        }
        
        private bool BlockFinished(int blockLevel)
        {
            if (_reader.TypeTag == Bms1Tag.MessageEnd || _reader.TypeTag == Bms1Tag.MessageStart)
            {
                return true;
            }
            return BlockNestingLevel <= blockLevel && (_reader.TypeTag == Bms1Tag.BlockEnd || _reader.TypeTag == Bms1Tag.BlockStart);
        }

        #region IBms1ObjectReader Members

        public Bms1Tag TypeTag
        {
            get { return _reader.TypeTag; }
        }

        public string ObjectType
        {
            get { return _attributes.ObjectType; }
        }

        public int BlockTypeId
        {
            get { return _reader.BlockTypeId; }
        }

        public string ObjectName
        {
            get { return _attributes.ObjectName; }
        }

        public bool IsCollection
        {
            get { return _attributes.CollectionElementCount >= 0; }
        }

        public int CollectionElementCount
        {
            get { return _attributes.CollectionElementCount; }
        }

        public bool IsCharacterType
        {
            get { return _attributes.IsCharacterType; }
        }

        public bool IsBlockType
        {
            get; private set;
        }

        public int BlockNestingLevel
        {
            get; private set;
        }

        public System.Collections.IList NameValueAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IList NamespaceAttributes
        {
            get { throw new NotImplementedException(); }
        }

        public bool EndOfMessage
        {
            get; private set;
        }

        public bool EndOfBlock
        {
            get; private set;
        }

        public bool ReadAttributes()
        {
            return !EndOfMessage && !EndOfBlock && ReadNextTag();
        }

        public bool IsArrayData
        {
            get { return _reader.IsArrayData; }
        }

        public int DataLength
        {
            get { return _reader.DataLength; }
        }

        public void SkipData()
        {
            _reader.SkipData(_stream);
        }

        public string ReadDataString()
        {
            return _reader.ReadDataString(_stream);
        }

        public uint ReadDataUInt()
        {
            return _reader.ReadDataUInt(_stream);
        }

        #endregion
    }
}
