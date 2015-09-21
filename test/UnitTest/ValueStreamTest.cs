namespace Remact.Net.Bms1Serializer.UnitTest
{
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
            _serializer.Writer.WriteInt(0);
            AssertBytesWritten(1);
            
            _serializer.Writer.WriteInt(-1);
            AssertBytesWritten(2);
            
            _serializer.Writer.WriteInt(-150);
            AssertBytesWritten(3);
            
            _serializer.Writer.WriteInt(-30000);
            AssertBytesWritten(3);
            
            _serializer.Writer.WriteInt(-33000);
            AssertBytesWritten(5);
            
            _serializer.Writer.WriteInt(1);
            AssertBytesWritten(2);
            
            _serializer.Writer.WriteInt(150);
            AssertBytesWritten(3);
            
            _serializer.Writer.WriteInt(30000);
            AssertBytesWritten(3);
            
            _serializer.Writer.WriteInt(33000);
            AssertBytesWritten(5);

            // Bool
            _serializer.Writer.WriteBool(true);
            AssertBytesWritten(1);
            _serializer.Writer.WriteBool(false);
            AssertBytesWritten(1);

            
            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);
            
            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();

            // Byte
            byte dataByte = 0;
            Assert.IsTrue(_serializer.Reader.ReadByte(ref dataByte));
            Assert.AreEqual(234, dataByte);
            Assert.IsTrue(_serializer.Reader.ReadByte(ref dataByte));
            Assert.AreEqual(0, dataByte);
            
            // Signed integer
            int dataInt = 2;
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(0, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-1, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-150, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-30000, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-33000, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(1, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(150, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(30000, dataInt);
            Assert.IsTrue(_serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(33000, dataInt);

            // Bool
            bool dataBool = false;
            Assert.IsTrue(_serializer.Reader.ReadBool(ref dataBool));
            Assert.IsTrue(dataBool);

            Assert.IsTrue(_serializer.Reader.ReadBool(ref dataBool));
            Assert.IsFalse(dataBool);

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

            _serializer.Writer.WriteUnicode('x');
            AssertBytesWritten(3);

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
            string dataString = null;
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual("Hello World", dataString);
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual("ÄäÖöÜü©≤£€∞¥™®÷×αµ≥", dataString);
            char dataChar = '\0';
            Assert.IsTrue(_serializer.Reader.ReadChar(ref dataChar));
            Assert.AreEqual('x', dataChar);
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual(string.Empty, dataString);
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual(null, dataString);
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual(longString, dataString);

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

            string dataString = null;
            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual("ABC\r\n", dataString);

            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.IsNull(dataString);

             char dataChar = 'x';
            Assert.IsTrue(_serializer.Reader.ReadChar(ref dataChar));
            Assert.AreEqual('\0', dataChar);

            Assert.IsTrue(_serializer.Reader.ReadString(ref dataString));
            Assert.AreEqual("©£€¥∞", dataString);

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

            _serializer.Writer.WriteByteArray(null);
            AssertBytesWritten(1);

            // Message end
            _serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            _serializer.Reader.Internal.ReadAttributes();
            byte[] dataBytes = null;
            Assert.IsTrue(_serializer.Reader.ReadByteArray(ref dataBytes));
            Assert.AreEqual(dataBytes.Length, 10);
            Assert.AreEqual(100, dataBytes[0]);
            Assert.AreEqual(190, dataBytes[9]);
            Assert.IsTrue(_serializer.Reader.ReadByteArray(ref dataBytes));
            Assert.AreEqual(dataBytes.Length, 250);
            Assert.IsTrue(_serializer.Reader.ReadByteArray(ref dataBytes));
            Assert.AreEqual(dataBytes.Length, 500);
            Assert.IsTrue(_serializer.Reader.ReadByteArray(ref dataBytes));
            Assert.IsNull(dataBytes);

            // Message end
            Assert.IsTrue(_serializer.Reader.Internal.EndOfBlock);
        }
    }
}

