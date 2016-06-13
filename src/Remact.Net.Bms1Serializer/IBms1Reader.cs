namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;
    
    /// <summary>
    /// The interface to read an object (block) and its fields from a stream.
    /// </summary>
    public interface IBms1Reader
    {
        /// <summary>
        /// The internal reader provides attributes of the next block or field to read.
        /// </summary>
        IBms1InternalReader Internal { get; }

        /// <summary>
        /// ReadBlock reads all fields of an object.
        /// Reads all data of an object, when readDto==null (block is skipped). 
        /// Reads up to the end of the object, when not all fields are read by readDto.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="readDto">A function returning a new object. The function has to read all fields in order. 
        /// It has to stop reading when <see cref="IBms1InternalReader.EndOfBlock"/> is true.</param>
        /// <returns>The new object of type T. Returns null, when no object is read because: EndOfBlock, EndOfMessage, readDto==null (block is skipped).</returns>
        T ReadBlock<T>(Func<T> readDto);

        /// <summary>
        /// Read a list of blocks of the same base type.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="blockFactory">A method to create and deserialize the objects.</param>
        /// <returns>A list of objects.</returns>
        List<T> ReadBlocks<T>(Func<IBms1Reader, T> blockFactory);

        /// <summary>
        /// Reads a boolean value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        bool ReadBool();

        /// <summary>
        /// Reads a byte value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// Reads a unsigned short int value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        UInt16 ReadUInt16();

        /// <summary>
        /// Reads a unsigned int value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        UInt32 ReadUInt32();

        /// <summary>
        /// Reads a short int value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int16 ReadInt16();

        /// <summary>
        /// Reads a int value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int32 ReadInt32();

        /// <summary>
        /// Reads a long int value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int64 ReadInt64();

        /// <summary>
        /// Reads a enum value (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        T ReadEnum<T>() where T : struct;
        Nullable<T> ReadNullable<T>() where T : struct;


        /// <summary>
        /// Reads a byte array (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        byte[] ReadByteArray();

        /// <summary>
        /// Reads a array of unsigned short  (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        UInt16[] ReadUInt16Array();

        /// <summary>
        /// Reads a array of unsigned int (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        UInt32[] ReadUInt32Array();

        /// <summary>
        /// Reads a array of short int (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int16[] ReadInt16Array();

        /// <summary>
        /// Reads a array of int (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int32[] ReadInt32Array();

        /// <summary>
        /// Reads a array of long int (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        Int64[] ReadInt64Array();


        /// <summary>
        /// Reads a character (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        char  ReadChar();

        /// <summary>
        /// Reads a UTF8 string (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        string ReadString();

        /// <summary>
        /// Reads a list of strings (when <see cref="IBms1InternalReader.EndOfBlock" is false)./>
        /// </summary>
        IList<string> ReadStrings();
//        
//        bool ReadDate(ref DateTime data);
//        bool ReadTime(ref DateTime data);
//        bool ReadDateTime(ref DateTime data);
    }
}
