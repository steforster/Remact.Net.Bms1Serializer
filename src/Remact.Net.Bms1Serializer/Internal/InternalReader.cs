namespace Remact.Net.Bms1Serializer.Internal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;

    internal class InternalReader : IBms1InternalReader, IMessageReader
    {
        private BinaryReader _stream;
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
                _attributes.ReadUntilNextValueOrFrameTag(_stream, _tagReader);

                if (!IsSupported(_tagReader.TypeTag))
                {
                    _tagReader.SkipData(_stream);
                    continue; // try if next tag is known
                }

                // known tag and its attributes read, data is available for read
                if (_tagReader.TypeTag == Bms1Tag.MessageFooter || _tagReader.TypeTag == Bms1Tag.MessageEnd)
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
                    if (_tagReader.TypeTag == Bms1Tag.BlockStart)
                    {
                        EndOfBlock = false;
                        IsBlockType = true;
                        BlockNestingLevel++;
                    }

                    if (_tagReader.TypeTag == Bms1Tag.BlockEnd)
                    {
                        EndOfBlock = true;
                        BlockNestingLevel--;
                    }
                }

                return !EndOfBlock;
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

        #region IBms1MessageReader Members
        
        // returns next message block type
        public int ReadMessageStart(BinaryReader binaryReader)
        {
            _stream = binaryReader;
            while (true)
            {
//                try
//                {
                    EndOfMessage = true;
                    EndOfBlock = true;
                    BlockNestingLevel = 0;
                    if (_tagReader.TypeTag != Bms1Tag.MessageStart)
                    {
                        ReadNextTag();
                    }

                    if (_tagReader.TypeTag == Bms1Tag.MessageStart)
                    {
                        if (ReadNextTag() && _tagReader.TypeTag == Bms1Tag.BlockStart)
                        {
                            return _tagReader.BlockTypeId; // valid message- and block start
                        }
                    }
                    // not a valid message- and block start found
                    _tagReader.SkipData(_stream);
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
            if (EndOfBlock || _tagReader.TypeTag != Bms1Tag.BlockStart || BlockNestingLevel != 1)
            {
                throw new Bms1Exception("not ready for message");
            }

            var ok = ReadBlock(dtoAction);

            while (_tagReader.TypeTag != Bms1Tag.MessageEnd && _tagReader.TypeTag != Bms1Tag.MessageStart)
            {
                // unknown blocks or values at end of message or resynchronization
                ReadNextTag();
                _tagReader.SkipData(_stream);
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
                if (_tagReader.TypeTag != Bms1Tag.BlockStart)
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
                _tagReader.SkipData(_stream);
            }

            if (BlockNestingLevel != thisBlockLevel - 1)
            {
                BlockNestingLevel = 0;
                throw new Bms1Exception("wrong block nesting = " + BlockNestingLevel + " after block level: " + thisBlockLevel, exception);
            }
            
            if (_tagReader.TypeTag != Bms1Tag.BlockEnd)
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
            if (_tagReader.TypeTag == Bms1Tag.MessageEnd || _tagReader.TypeTag == Bms1Tag.MessageStart)
            {
                return true;
            }
            return BlockNestingLevel <= blockLevel && (_tagReader.TypeTag == Bms1Tag.BlockEnd || _tagReader.TypeTag == Bms1Tag.BlockStart);
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

        public Bms1Tag TypeTag
        {
            get { return _tagReader.TypeTag; }
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
            _tagReader.SkipData(_stream);
        }

        public string ReadDataString()
        {
            return _tagReader.ReadDataString(_stream);
        }

        public uint ReadDataUInt()
        {
            return _tagReader.ReadDataUInt(_stream);
        }

        public bool ThrowError(string message)
        {
            var s = string.Empty;
            if (_attributes.CollectionElementCount >= 0)
            {
                s = "collection["+_attributes.CollectionElementCount+"]";
            }
            if (_attributes.IsCharacterType)
            {
                s += ", char";
            }
            s = string.Format("{0}. Tag={1}, DataLength={2}, Attributes='{3}'", message, _tagReader.TagByte, _tagReader.DataLength, s);
            throw new Bms1Exception(s);
        }
        
        #endregion
    }
}
