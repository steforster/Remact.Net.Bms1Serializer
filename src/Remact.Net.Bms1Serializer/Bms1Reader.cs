namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.IO;
    using System.Text;

    public class Bms1Reader
    {
        private BinaryReader _stream;

        private Bms1Attributes _attributes;

        private Bms1Tag _tag;
        
        private int _nestedBlocks;

        // private Encoding _asciiEncoding;// ASCII Encoding not available in PortableFramework


        public Bms1Reader(BinaryReader streamReader)
        {
            _stream = streamReader;
            _tag = new Bms1Tag();
            _attributes = new Bms1Attributes();
            EndOfMessage = true;
            EndOfBlock = true;
        }

        public bool EndOfMessage { get; private set; } // before message start or after message end
        public bool EndOfBlock { get; private set; } // before block start or after block end


        // returns false, when EndOfBlock or EndOfMessage == true after reading next tag
        private bool ReadNextTag()
        {
            while (true)
            {
                _attributes.Clear();
                _attributes.ReadUntilNextValueOrFrameTag(_stream, _tag);

                if (!IsSupported(_tag.Type))
                {
                    _tag.SkipData(_stream);
                    continue; // try if next tag is known
                }

                // known tag and its attributes read, data is available for read
                if (_tag.Type == Tag.MessageFooter || _tag.Type == Tag.MessageEnd)
                {
                    if (_nestedBlocks != 0)
                    {
                        _nestedBlocks = 0;
                        throw new Bms1Exception("wrong block nesting at end of message: " + _nestedBlocks);
                    }
                    EndOfMessage = true;
                    EndOfBlock = true;
                }

                if (!EndOfMessage)
                {
                    if (_tag.Type == Tag.BlockStart)
                    {
                        EndOfBlock = false;
                        _nestedBlocks++;
                    }

                    if (_tag.Type == Tag.BlockEnd)
                    {
                        EndOfBlock = true;
                        _nestedBlocks--;
                    }
                }

                return !EndOfMessage && !EndOfBlock;
            }
        }

        private bool IsSupported(Tag tagType)
        {
            if (_attributes.TagSetNumber != 0)
            {
                return false;
            }

            switch (tagType)
            {
                case Tag.BoolFalse:
                case Tag.BoolTrue:
                case Tag.UByte:
                case Tag.SInt: // TODO arrays
                case Tag.String:

                case Tag.MessageStart:
                case Tag.MessageFooter:
                case Tag.MessageEnd:
                case Tag.BlockStart:
                case Tag.BlockEnd:
                    return true;

                default:
                    return false;
            }
        }

        // returns next message block type
        public int ReadMessageStart(out Bms1Attributes messageAttributes)
        {
            while (true)
            {
                try
                {
                    EndOfMessage = true;
                    EndOfBlock = true;
                    _nestedBlocks = 0;
                    if (_tag.Type != Tag.MessageStart)
                    {
                        ReadNextTag();
                    }
                    
                    if (_tag.Type == Tag.MessageStart)
                    {
                        if (ReadNextTag() && _tag.Type == Tag.BlockStart)
                        {
                            messageAttributes = _attributes;
                            return _tag.BlockTypeId; // valid message- and block start
                        }
                    }
                    // not a valid message- and block start found
                    _tag.SkipData(_stream);
                }
                catch (Bms1Exception ex)
                {
                    // Invalid data or resynchronizing
                }
            }// while
        }

        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadMessage(ref IBms1Block message)
        {
            if (EndOfMessage || EndOfBlock)
            {
                throw new Bms1Exception("not ready for message");
            }

            var ok = ReadBlock(message);
            
            while (_tag.Type != Tag.MessageEnd && _tag.Type != Tag.MessageStart)
            {
                // unknown blocks or values at end of message or resynchronization
                ReadNextTag();
                _tag.SkipData(_stream);
            }
            return ok;
        }

        // returns next block type
        public int ReadBlockStart(out Bms1Attributes messageAttributes)
        {
            if (EndOfMessage || !ReadNextTag() || _tag.Type != Tag.BlockStart)
            {
                throw new Bms1Exception("wrong block start");
            }
            
            messageAttributes = _attributes;
            return _tag.BlockTypeId;
        }
        
        // returns false, when not read because: NoData(null), not matching type, EndOfBlock, EndOfMessage
        public bool ReadBlock(IBms1Block blockDto)
        {
            if (EndOfMessage)
            {
                return false;
            }
            
            if (_tag.Type != Tag.BlockStart)
            {
                ReadNextTag();
            }
            
            if (EndOfBlock || _tag.Type != Tag.BlockStart)
            {
                throw new Bms1Exception("wrong block start");
            }
            
            var thisBlockLevel = _nestedBlocks;
            
            bool ok = true;
            try
            {
                blockDto.Bms1Read (this, _tag.BlockTypeId, _attributes);
                
                if (_nestedBlocks != thisBlockLevel)
                {
                    _nestedBlocks = 0;
                    throw new Bms1Exception("wrong block nesting = " + _nestedBlocks + " at end of block level: " + thisBlockLevel);
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
                _tag.SkipData(_stream);
            }
            
            if (_nestedBlocks != thisBlockLevel - 1)
            {
                _nestedBlocks = 0;
                throw new Bms1Exception("wrong block nesting = " + _nestedBlocks + " at after block level: " + thisBlockLevel);
            }
            return ok;
        }
        
        private bool BlockFinished(int blockLevel)
        {
            if (_tag.Type == Tag.MessageEnd || _tag.Type == Tag.MessageStart)
            {
                return true;
            }
            return _nestedBlocks <= blockLevel && (_tag.Type == Tag.BlockEnd || _tag.Type == Tag.BlockStart);
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadBool(ref bool data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.DataLength == 0 && !_tag.IsArrayValue)
            {
                if (_tag.Type == Tag.BoolFalse)
                {
                    data = false;
                    return true;
                }
                else if (_tag.Type == Tag.BoolTrue)
                {
                    data = true;
                    return true;
                }
            }
            throw new Bms1Exception("cannot read bool");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByte(ref byte data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.Type == Tag.UByte && !_tag.IsArrayValue)
            {
                if (_tag.DataLength == 0)
                {
                    data = 0;
                    return true;
                }
                else if (_tag.DataLength == 1)
                {
                    data = _stream.ReadByte();
                    return true;
                }
            }
            throw new Bms1Exception("cannot read byte");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadByteArray(ref byte[] data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.Type == Tag.UByte && _tag.IsArrayValue)
            {
                data = _stream.ReadBytes(_tag.DataLength);
                return true;
            }
            throw new Bms1Exception("cannot read byte array");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadInt(ref int data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.Type == Tag.SInt && !_tag.IsArrayValue)
            {
                switch (_tag.DataLength)
                {
                    case 0: data = 0; return true;
                    case 1: data = _stream.ReadSByte(); return true;
                    case 2: data = _stream.ReadInt16(); return true;
                    case 4: data = _stream.ReadInt32(); return true;
                }
            }
            throw new Bms1Exception("cannot read int");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadString(ref string data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.Type == Tag.String && _tag.IsArrayValue)
            {
                data = _tag.ReadDataString(_stream);
                return true;
            }

            if (_tag.Type == Tag.UByte && _tag.IsArrayValue && _attributes.IsSet(Bms1Attr.CharacterType))
            {
                var buffer = _stream.ReadBytes(_tag.DataLength);
                //data = _asciiEncoding.GetString(buffer, 0, buffer.Length);
                data = Encoding.UTF8.GetString(buffer, 0, buffer.Length); // fallback to UTF8 because ASCII is not available in portable framework.
                return true;
            }

            if (_tag.Type == Tag.UShort && _tag.IsArrayValue && _attributes.IsSet(Bms1Attr.CharacterType))
            {
                var buffer = _stream.ReadBytes(_tag.DataLength);
                data = Encoding.Unicode.GetString(buffer, 0, buffer.Length);
                return true;
            }

            throw new Bms1Exception("cannot read string");
        }

        // returns false, when not read because: NoData(null), EndOfBlock, EndOfMessage
        public bool ReadChar(ref char data)
        {
            if (EndOfMessage || EndOfBlock || !ReadNextTag())
            {
                return false;
            }

            if (_tag.Type == Tag.String && !_tag.IsArrayValue)
            {
                return ConvertToChar(ref data);
            }

            if (_tag.Type == Tag.UByte && !_tag.IsArrayValue && _attributes.IsSet(Bms1Attr.CharacterType))
            {
                return ConvertToChar(ref data);
            }

            if (_tag.Type == Tag.UShort && !_tag.IsArrayValue && _attributes.IsSet(Bms1Attr.CharacterType))
            {
                return ConvertToChar(ref data);
            }

            throw new Bms1Exception("cannot read char");
        }

        public bool ConvertToChar(ref char data)
        {
            if (_tag.DataLength == 0)
            {
                data = '\0';
                return true;
            }
            else if (_tag.DataLength == 0)
            {   // convert one ASCII byte to unicode char
                //byte[] buffer = new byte[1];
                //buffer[0] = _stream.ReadByte();
                //var s = _asciiEncoding.GetString(buffer, 0, 1);
                //data = s[0];
                // ASCII Encoding not available in PortableFramework
                short b = _stream.ReadByte();
                data = (char)b;
                return true;
            }
            else if  (_tag.DataLength == 2)
            {
                data = (char)_stream.ReadInt16();
            }
            return false;
        }
    }
}
