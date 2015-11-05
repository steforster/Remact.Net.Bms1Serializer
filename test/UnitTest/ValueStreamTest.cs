namespace Remact.Net.Bms1Serializer.UnitTest
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NUnit.Framework;

    using System;
    using System.IO;
    using Remact.Net.Bms1Serializer.Internal;

    [TestClass]
    [TestFixture]
    public class ValueStreamTest
    {
        MemoryStream _stream;
        int _latestPosition;
        TestSerializer _serializer;
        
        [TestInitialize]
        [SetUp]
        public void TestInitialize()
        {
            _stream = new MemoryStream();
            _serializer = new TestSerializer(_stream);
            _latestPosition = 0;
        }
        
        
        void AssertBytesWritten(int count)
        {
            int pos = (int)_stream.Position;
            Assert.AreEqual(count, pos - _latestPosition, "not the expected bytes written");
            _latestPosition = pos;
        }
        
        [TestMethod]
        [Test]
        public void SerializeSimpleTypes()
        {
            // Write data -------------------------------------
            // Byte
            _serializer.Writer.WriteByte(234);
            AssertBytesWritten(2);
            
            _serializer.Writer.WriteByte(0);
            AssertBytesWritten(1);
            
            // Signed integer
            _serializer.Writer.WriteInt16(0); AssertBytesWritten(1);
            _serializer.Writer.WriteInt16(-1); AssertBytesWritten(2);
            _serializer.Writer.WriteInt16(10); AssertBytesWritten(2);
            _serializer.Writer.WriteInt16(-167); AssertBytesWritten(3);
            _serializer.Writer.WriteInt16(1234); AssertBytesWritten(3);

            _serializer.Writer.WriteInt32(0); AssertBytesWritten(1);
            _serializer.Writer.WriteInt32(-1); AssertBytesWritten(2);
            _serializer.Writer.WriteInt32(-150); AssertBytesWritten(3);
            _serializer.Writer.WriteInt32(-30000); AssertBytesWritten(3);
            _serializer.Writer.WriteInt32(-33000); AssertBytesWritten(5);
            _serializer.Writer.WriteInt32(1); AssertBytesWritten(2);
            _serializer.Writer.WriteInt32(150); AssertBytesWritten(3);
            _serializer.Writer.WriteInt32(30000); AssertBytesWritten(3);
            _serializer.Writer.WriteInt32(33000); AssertBytesWritten(5);

            _serializer.Writer.WriteInt64(0); AssertBytesWritten(1);
            _serializer.Writer.WriteInt64(-1); AssertBytesWritten(2);
            _serializer.Writer.WriteInt64(10); AssertBytesWritten(2);
            _serializer.Writer.WriteInt64(-324); AssertBytesWritten(3);
            _serializer.Writer.WriteInt64(2345); AssertBytesWritten(3);
            _serializer.Writer.WriteInt64(-64000); AssertBytesWritten(5);
            _serializer.Writer.WriteInt64(int.MaxValue); AssertBytesWritten(5);
            _serializer.Writer.WriteInt64(long.MinValue); AssertBytesWritten(9);
            _serializer.Writer.WriteInt64(long.MaxValue); AssertBytesWritten(9);

            // Bool
            _serializer.Writer.WriteBool(true); AssertBytesWritten(1);
            _serializer.Writer.WriteBool(false); AssertBytesWritten(1);


            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            // Byte
            Assert.AreEqual(234, _serializer.Reader.ReadByte());
            Assert.AreEqual(0, _serializer.Reader.ReadByte());

            // Signed integer
            Assert.AreEqual(0, _serializer.Reader.ReadInt16());
            Assert.AreEqual(-1, _serializer.Reader.ReadInt16());
            Assert.AreEqual(10, _serializer.Reader.ReadInt16());
            Assert.AreEqual(-167, _serializer.Reader.ReadInt16());
            Assert.AreEqual(1234, _serializer.Reader.ReadInt16());

            Assert.AreEqual(0, _serializer.Reader.ReadInt32());
            Assert.AreEqual(-1, _serializer.Reader.ReadInt32());
            Assert.AreEqual(-150, _serializer.Reader.ReadInt32());
            Assert.AreEqual(-30000, _serializer.Reader.ReadInt32());
            Assert.AreEqual(-33000, _serializer.Reader.ReadInt32());
            Assert.AreEqual(1, _serializer.Reader.ReadInt32());
            Assert.AreEqual(150, _serializer.Reader.ReadInt32());
            Assert.AreEqual(30000, _serializer.Reader.ReadInt32());
            Assert.AreEqual(33000, _serializer.Reader.ReadInt32());

            Assert.AreEqual(0, _serializer.Reader.ReadInt64());
            Assert.AreEqual(-1, _serializer.Reader.ReadInt64());
            Assert.AreEqual(10, _serializer.Reader.ReadInt64());
            Assert.AreEqual(-324, _serializer.Reader.ReadInt64());
            Assert.AreEqual(2345, _serializer.Reader.ReadInt64());
            Assert.AreEqual(-64000, _serializer.Reader.ReadInt64());
            Assert.AreEqual(int.MaxValue, _serializer.Reader.ReadInt64());
            Assert.AreEqual(long.MinValue, _serializer.Reader.ReadInt64());
            Assert.AreEqual(long.MaxValue, _serializer.Reader.ReadInt64());

            // Bool
            Assert.IsTrue(_serializer.Reader.ReadBool());
            Assert.IsFalse(_serializer.Reader.ReadBool());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }


        [TestMethod]
        [Test]
        public void SerializeNullableTypes()
        {
            // Write data -------------------------------------
            // Byte
            byte? dataByte = 234; _serializer.Writer.WriteByte(dataByte); AssertBytesWritten(2);
            dataByte = 0; _serializer.Writer.WriteByte(dataByte); AssertBytesWritten(1);
            dataByte = null; _serializer.Writer.WriteByte(dataByte); AssertBytesWritten(1);

            // Signed integer
            Int16? dataInt16 = 0; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(1);
            dataInt16 = SByte.MinValue; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(2);
            dataInt16 = SByte.MaxValue; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(2);
            dataInt16 = Int16.MinValue; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(3);
            dataInt16 = Int16.MaxValue; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(3);
            dataInt16 = null; _serializer.Writer.WriteInt16(dataInt16); AssertBytesWritten(1);

            Int32? dataInt32 = 0; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(1);
            dataInt32 = -1; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(2);
            dataInt32 = -150; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(3);
            dataInt32 = -30000; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(3);
            dataInt32 = -33000; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(5);
            dataInt32 = 1; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(2);
            dataInt32 = 150; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(3);
            dataInt32 = 30000; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(3);
            dataInt32 = 33000; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(5);
            dataInt32 = null; _serializer.Writer.WriteInt32(dataInt32); AssertBytesWritten(1);

            Int64? dataInt64 = 0; _serializer.Writer.WriteInt64(dataInt64); AssertBytesWritten(1);
            dataInt64 = int.MaxValue; _serializer.Writer.WriteInt64(dataInt64); AssertBytesWritten(5);
            dataInt64 = long.MinValue; _serializer.Writer.WriteInt64(dataInt64); AssertBytesWritten(9);
            dataInt64 = long.MaxValue; _serializer.Writer.WriteInt64(dataInt64); AssertBytesWritten(9);
            dataInt64 = null; _serializer.Writer.WriteInt64(dataInt64); AssertBytesWritten(1);

            // Bool
            bool? dataBool = true; _serializer.Writer.WriteBool(dataBool); AssertBytesWritten(1);
            dataBool = false; _serializer.Writer.WriteBool(dataBool); AssertBytesWritten(1);
            dataBool = null; _serializer.Writer.WriteBool(dataBool); AssertBytesWritten(1);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            // Byte
            Assert.AreEqual((byte)234, _serializer.Reader.ReadNullable<byte>());
            Assert.AreEqual((byte)0, _serializer.Reader.ReadNullable<byte>());
            Assert.IsNull(_serializer.Reader.ReadNullable<byte>());

            // Signed integer
            Assert.AreEqual((Int16)0, _serializer.Reader.ReadNullable<Int16>());
            Assert.AreEqual((Int16)SByte.MinValue, _serializer.Reader.ReadNullable<Int16>());
            Assert.AreEqual((Int16)SByte.MaxValue, _serializer.Reader.ReadNullable<Int16>());
            Assert.AreEqual(Int16.MinValue, _serializer.Reader.ReadNullable<Int16>());
            Assert.AreEqual(Int16.MaxValue, _serializer.Reader.ReadNullable<Int16>());
            Assert.IsNull(_serializer.Reader.ReadNullable<Int16>());

            Assert.AreEqual(0, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(-1, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(-150, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(-30000, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(-33000, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(1, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(150, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(30000, _serializer.Reader.ReadNullable<int>());
            Assert.AreEqual(33000, _serializer.Reader.ReadNullable<int>());
            Assert.IsNull(_serializer.Reader.ReadNullable<int>());

            Assert.AreEqual((Int64)0, _serializer.Reader.ReadNullable<Int64>());
            Assert.AreEqual((Int64)int.MaxValue, _serializer.Reader.ReadNullable<Int64>());
            Assert.AreEqual((Int64)long.MinValue, _serializer.Reader.ReadNullable<Int64>());
            Assert.AreEqual((Int64)long.MaxValue, _serializer.Reader.ReadNullable<Int64>());
            Assert.IsNull(_serializer.Reader.ReadNullable<Int64>());

            // Bool
            Assert.IsTrue((bool)_serializer.Reader.ReadNullable<bool>());
            Assert.IsFalse((bool)_serializer.Reader.ReadNullable<bool>());
            Assert.IsNull(_serializer.Reader.ReadNullable<bool>());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }




        [TestMethod]
        [Test]
        public void SerializeUnsignedTypes()
        {
            // Write data -------------------------------------
            UInt16? dataUInt16 = 0; _serializer.Writer.WriteUInt16(dataUInt16); AssertBytesWritten(1);
            _serializer.Writer.WriteUInt16(dataUInt16.Value); AssertBytesWritten(1);
            dataUInt16 = Byte.MaxValue; _serializer.Writer.WriteUInt16(dataUInt16); AssertBytesWritten(2);
            _serializer.Writer.WriteUInt16(dataUInt16.Value); AssertBytesWritten(2);
            dataUInt16 = UInt16.MaxValue; _serializer.Writer.WriteUInt16(dataUInt16); AssertBytesWritten(3);
            _serializer.Writer.WriteUInt16(dataUInt16.Value); AssertBytesWritten(3);
            dataUInt16 = null; _serializer.Writer.WriteUInt16(dataUInt16); AssertBytesWritten(1);

            UInt32? dataUInt32 = 0; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(1);
            _serializer.Writer.WriteUInt32(dataUInt32.Value); AssertBytesWritten(1);
            dataUInt32 = 1; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(2);
            _serializer.Writer.WriteUInt32(dataUInt32.Value); AssertBytesWritten(2);
            dataUInt32 = 150; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(2);
            _serializer.Writer.WriteUInt32(dataUInt32.Value); AssertBytesWritten(2);
            dataUInt32 = 33000; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(3);
            _serializer.Writer.WriteUInt32(dataUInt32.Value); AssertBytesWritten(3);
            dataUInt32 = UInt32.MaxValue; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(5);
            _serializer.Writer.WriteUInt32(dataUInt32.Value); AssertBytesWritten(5);
            dataUInt32 = null; _serializer.Writer.WriteUInt32(dataUInt32); AssertBytesWritten(1);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            // Unsigned integer
            Assert.AreEqual((UInt16)0, _serializer.Reader.ReadNullable<UInt16>());
            Assert.AreEqual((UInt16)0, _serializer.Reader.ReadUInt16());
            Assert.AreEqual((UInt16)Byte.MaxValue, _serializer.Reader.ReadNullable<UInt16>());
            Assert.AreEqual((UInt16)Byte.MaxValue, _serializer.Reader.ReadUInt16());
            Assert.AreEqual(UInt16.MaxValue, _serializer.Reader.ReadNullable<UInt16>());
            Assert.AreEqual(UInt16.MaxValue, _serializer.Reader.ReadUInt16());
            Assert.IsNull(_serializer.Reader.ReadNullable<UInt16>());

            Assert.AreEqual((UInt32)0, _serializer.Reader.ReadNullable<UInt32>());
            Assert.AreEqual((UInt32)0, _serializer.Reader.ReadUInt32());
            Assert.AreEqual((UInt32)1, _serializer.Reader.ReadNullable<UInt32>());
            Assert.AreEqual((UInt32)1, _serializer.Reader.ReadUInt32());
            Assert.AreEqual((UInt32)150, _serializer.Reader.ReadNullable<UInt32>());
            Assert.AreEqual((UInt32)150, _serializer.Reader.ReadUInt32());
            Assert.AreEqual((UInt32)33000, _serializer.Reader.ReadNullable<UInt32>());
            Assert.AreEqual((UInt32)33000, _serializer.Reader.ReadUInt32());
            Assert.AreEqual(UInt32.MaxValue, _serializer.Reader.ReadNullable<UInt32>());
            Assert.AreEqual(UInt32.MaxValue, _serializer.Reader.ReadUInt32());
            Assert.IsNull(_serializer.Reader.ReadNullable<UInt32>());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }


        [TestMethod]
        [Test]
        public void SerializeStrings()
        {
            // Write data -------------------------------------
            _serializer.Writer.WriteString("Hello World");
            AssertBytesWritten(13);

            _serializer.Writer.WriteString("ÄäÖöÜü©≤£€∞¥™®÷×αµ≥");
            AssertBytesWritten(54);

            _serializer.Writer.WriteUnicode('x'); AssertBytesWritten(3);
            _serializer.Writer.WriteUnicode('®'); AssertBytesWritten(3); // is read as nullable
            char? nchar = 'α'; _serializer.Writer.WriteUnicode(nchar); AssertBytesWritten(3);
            nchar = null; _serializer.Writer.WriteUnicode(nchar); AssertBytesWritten(1);

            _serializer.Writer.WriteString(string.Empty);
            AssertBytesWritten(1);

            _serializer.Writer.WriteString(null);
            AssertBytesWritten(1);

            string longString = new string('@', 260);
            _serializer.Writer.WriteString(longString);
            AssertBytesWritten(265);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();
            Assert.AreEqual("Hello World", _serializer.Reader.ReadString());
            Assert.AreEqual("ÄäÖöÜü©≤£€∞¥™®÷×αµ≥", _serializer.Reader.ReadString());
            Assert.AreEqual('x', _serializer.Reader.ReadChar());
            Assert.AreEqual('®', _serializer.Reader.ReadNullable<char>());
            Assert.AreEqual('α', _serializer.Reader.ReadNullable<char>());
            Assert.AreEqual(null, _serializer.Reader.ReadNullable<char>());
            Assert.AreEqual(string.Empty, _serializer.Reader.ReadString());
            Assert.AreEqual(null, _serializer.Reader.ReadString());
            Assert.AreEqual(longString, _serializer.Reader.ReadString());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }


        [TestMethod]
        [Test]
        public void SerializeCharArray()
        {
            // Write data -------------------------------------
            Assert.AreEqual(-1, _serializer.Writer.Internal.CollectionElementCount);

            var bytes = new byte[5];
            bytes[0] = (byte)'A';
            bytes[1] = (byte)'B';
            bytes[2] = (byte)'C';
            bytes[3] = (byte)'\r';
            bytes[4] = (byte)'\n';
            _serializer.Writer.Internal.IsCharacterType = true;
            _serializer.Writer.WriteByteArray(bytes);
            AssertBytesWritten(8); // attribute + tag + length + 5char
            Assert.IsFalse(_serializer.Writer.Internal.IsCharacterType);
            Assert.AreEqual(-1, _serializer.Writer.Internal.CollectionElementCount);

            _serializer.Writer.Internal.IsCharacterType = true;
            _serializer.Writer.WriteByteArray(null);
            AssertBytesWritten(2);
            Assert.IsFalse(_serializer.Writer.Internal.IsCharacterType);

            _serializer.Writer.Internal.IsCharacterType = true;
            _serializer.Writer.WriteByte(0);
            AssertBytesWritten(2);
            Assert.IsFalse(_serializer.Writer.Internal.IsCharacterType);

            var chars = new ushort[5];
            chars[0] = '©';
            chars[1] = '£';
            chars[2] = '€';
            chars[3] = '¥';
            chars[4] = '∞';
            _serializer.Writer.Internal.IsCharacterType = true;
            _serializer.Writer.WriteUInt16Array(chars);
            AssertBytesWritten(13); // attribute + tag + length + 10 bytes wchar
            Assert.IsFalse(_serializer.Writer.Internal.IsCharacterType);
            Assert.AreEqual(-1, _serializer.Writer.Internal.CollectionElementCount);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);
            Assert.IsFalse(_serializer.Writer.Internal.IsCharacterType);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            Assert.AreEqual("ABC\r\n", _serializer.Reader.ReadString());
            Assert.IsNull(_serializer.Reader.ReadString());
            Assert.AreEqual('\0', _serializer.Reader.ReadChar());
            Assert.AreEqual("©£€¥∞", _serializer.Reader.ReadString());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }


        [TestMethod]
        [Test]
        public void SerializeByteArray()
        {
            // Write data -------------------------------------
            var bytes = new byte[10];
            bytes[0] = 100;
            bytes[9] = 190;
            _serializer.Writer.WriteByteArray(bytes);
            AssertBytesWritten(12);

            _serializer.Writer.WriteByteArray(new byte[250]);
            AssertBytesWritten(252);

            _serializer.Writer.WriteByteArray(new byte[500]);
            AssertBytesWritten(505);

            _serializer.Writer.WriteByteArray(new byte[0]);
            AssertBytesWritten(2);

            _serializer.Writer.WriteByteArray(null);
            AssertBytesWritten(1);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();
            byte[] dataBytes = _serializer.Reader.ReadByteArray();
            Assert.AreEqual(dataBytes.Length, 10);
            Assert.AreEqual(100, dataBytes[0]);
            Assert.AreEqual(190, dataBytes[9]);

            dataBytes = _serializer.Reader.ReadByteArray();
            Assert.AreEqual(dataBytes.Length, 250);

            dataBytes = _serializer.Reader.ReadByteArray();
            Assert.AreEqual(dataBytes.Length, 500);

            dataBytes = _serializer.Reader.ReadByteArray();
            Assert.AreEqual(dataBytes.Length, 0);

            dataBytes = _serializer.Reader.ReadByteArray();
            Assert.IsNull(dataBytes);

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }


        [TestMethod]
        [Test]
        public void SerializeArrays()
        {
            // Write data -------------------------------------
            var arrayUInt16 = new UInt16[] { 0, 900, 32000 };
           _serializer.Writer.WriteUInt16Array(arrayUInt16);
            AssertBytesWritten(8);

            var arrayUInt32 = new UInt32[] { 0, 900, 32000 };
           _serializer.Writer.WriteUInt32Array(arrayUInt32);
            AssertBytesWritten(14);

            var arrayInt16 = new Int16[] { -700, 0, 32000 };
           _serializer.Writer.WriteInt16Array(arrayInt16);
            AssertBytesWritten(8);

            var arrayInt32 = new Int32[] { -700, 0, 32000 };
           _serializer.Writer.WriteInt32Array(arrayInt32);
            AssertBytesWritten(14);

            var arrayInt64 = new Int64[] { -700, 0, 32000 };
           _serializer.Writer.WriteInt64Array(arrayInt64);
            AssertBytesWritten(26);

            _serializer.Writer.WriteUInt16Array(null); AssertBytesWritten(1);
            _serializer.Writer.WriteUInt32Array(null); AssertBytesWritten(1);
            _serializer.Writer.WriteInt16Array(null); AssertBytesWritten(1);
            _serializer.Writer.WriteInt32Array(null); AssertBytesWritten(1);
            _serializer.Writer.WriteInt64Array(null); AssertBytesWritten(1);


            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            Assert.IsTrue(arrayUInt16.SequenceEqual(_serializer.Reader.ReadUInt16Array()));
            Assert.IsTrue(arrayUInt32.SequenceEqual(_serializer.Reader.ReadUInt32Array()));
            Assert.IsTrue(arrayInt16.SequenceEqual(_serializer.Reader.ReadInt16Array()));
            Assert.IsTrue(arrayInt32.SequenceEqual(_serializer.Reader.ReadInt32Array()));
            Assert.IsTrue(arrayInt64.SequenceEqual(_serializer.Reader.ReadInt64Array()));

            Assert.IsNull(_serializer.Reader.ReadUInt16Array());
            Assert.IsNull(_serializer.Reader.ReadUInt32Array());
            Assert.IsNull(_serializer.Reader.ReadInt16Array());
            Assert.IsNull(_serializer.Reader.ReadInt32Array());
            Assert.IsNull(_serializer.Reader.ReadInt64Array());

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }

        // TODO collections
    }
}

