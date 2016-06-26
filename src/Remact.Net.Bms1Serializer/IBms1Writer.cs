namespace Remact.Net.Bms1Serializer
{
    using System;
    using System.Collections.Generic;
    using Remact.Net.Bms1Serializer.Internal;

    /// <summary>
    /// The interface to write an object (block) and its fields to a stream.
    /// </summary>
    public interface IBms1Writer
    {
        /// <summary>
        /// The internal writers allows to set attributes of the next block or field.
        /// </summary>
        IBms1InternalWriter Internal { get; }

        /// <summary>
        /// Write an object. Writes null when writeDtoAction is null.
        /// </summary>
        /// <param name="blockTypeId">The block type ID (object type ID) to distiguish derieved types of a base object.</param>
        /// <param name="writeDtoAction">The lambda expression to write the fields of an object in order.</param>
        void WriteBlock(UInt16 blockTypeId, Action writeDtoAction);

        /// <summary>
        /// Write an object. Writes null when writeDtoAction is null. Does not set a blockTypeId attribute.
        /// </summary>
        /// <param name="writeDtoAction">The lambda expression to write the fields of an object in order.</param>
        void WriteBlock(Action writeDtoAction);

        /// <summary>
        /// Write an enumerable of blocks. Writes null when data or writeBlockAction is null. 
        /// </summary>
        /// <typeparam name="T">The base block type.</typeparam>
        /// <param name="baseBlockTypeId">The block ID of the base block.</param>
        /// <param name="data">The enumerable of blocks.</param>
        /// <param name="writeBlockAction">The action th write this type of base block.</param>
        void WriteBlocks<T>(UInt16 baseBlockTypeId, IEnumerable<T> data, Action<object, IBms1Writer> writeBlockAction);

        // TODO: WriteObject(); the objects supported by BMS natively

        /// <summary>
        /// Writes a boolean value.
        /// </summary>
        void WriteBool(bool data);

        /// <summary>
        /// Writes a boolean value or null.
        /// </summary>
        void WriteBool(bool? data);

        /// <summary>
        /// Writes a byte value.
        /// </summary>
        void WriteByte(byte data);

        /// <summary>
        /// Writes a byte array.
        /// </summary>
        void WriteByteArray(byte[] data);

        /// <summary>
        /// Writes a list of bytes.
        /// </summary>
        void WriteByteArray(IEnumerable<Byte> data);

        /// <summary>
        /// Writes a byte or null.
        /// </summary>
        void WriteByte(byte? data);

        /// <summary>
        /// Writes a short unsigned int value.
        /// </summary>
        void WriteUInt16(UInt16 data);

        /// <summary>
        /// Writes a list of short unsigned int.
        /// </summary>
        void WriteUInt16Array(IEnumerable<UInt16> data);

        /// <summary>
        /// Writes a short unsigned int or null.
        /// </summary>
        void WriteUInt16(UInt16? data);

        /// <summary>
        /// Writes a unsigned int value.
        /// </summary>
        void WriteUInt32(UInt32 data);

        /// <summary>
        /// Writes a list of unsigned int.
        /// </summary>
        void WriteUInt32Array(IEnumerable<UInt32> data);

        /// <summary>
        /// Writes a unsigned int or null.
        /// </summary>
        void WriteUInt32(UInt32? data);

        /// <summary>
        /// Writes a short int value.
        /// </summary>
        void WriteInt16(Int16 data);

        /// <summary>
        /// Writes a list of short int.
        /// </summary>
        void WriteInt16Array(IEnumerable<Int16> data);

        /// <summary>
        /// Writes a short int or null.
        /// </summary>
        void WriteInt16(Int16? data);

        /// <summary>
        /// Writes a int value.
        /// </summary>
        void WriteInt32(Int32 data);

        /// <summary>
        /// Writes a list of int.
        /// </summary>
        void WriteInt32Array(IEnumerable<Int32> data);

        /// <summary>
        /// Writes a int or null.
        /// </summary>
        void WriteInt32(Int32? data);

        /// <summary>
        /// Writes a long int value.
        /// </summary>
        void WriteInt64(Int64 data);

        /// <summary>
        /// Writes a list of long int.
        /// </summary>
        void WriteInt64Array(IEnumerable<Int64> data);

        /// <summary>
        /// Writes a long int or null.
        /// </summary>
        void WriteInt64(Int64? data);

        /// <summary>
        /// Writes a enum value.
        /// </summary>
        void WriteEnum(Enum data);

        /// <summary>
        /// Writes a character value.
        /// </summary>
        void WriteUnicode(char data);

        /// <summary>
        /// Writes a character value or null.
        /// </summary>
        void WriteUnicode(char? data);

        /// <summary>
        /// Writes a UTF8 string.
        /// </summary>
        void WriteString(string data);

        /// <summary>
        /// Writes a list of strings.
        /// </summary>
        void WriteStrings(IList<string> data);

        //        void WriteDate(DateTime data);
        //        void WriteTime(DateTime data);
        //        void WriteDateTime(DateTime data);
    }
}
