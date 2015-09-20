namespace Remact.Net.Bms1Serializer.NUnitTest
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using Remact.Net.Bms1Serializer.Internal;

    [TestFixture]
    public class ReaderTest
    {
        MemoryStream _stream;
        int _latestPosition;
        
        void AssertBytesWritten(int count)
        {
            int pos = (int)_stream.Position;
            Assert.AreEqual(count, pos - _latestPosition, "not the expected bytes written");
            _latestPosition = pos;
        }
        
        [Test]
        public void ReadSimpleTypes()
        {
            // Write data -------------------------------------
            _stream = new MemoryStream();
            var serializer = new TestSerializer(_stream);
            serializer.Writer.WriteByte(234);
            AssertBytesWritten(2);
            
            serializer.Writer.WriteByte(0);
            AssertBytesWritten(1);
            
            // Signed integer
            serializer.Writer.WriteInt(0);
            AssertBytesWritten(1);
            
            serializer.Writer.WriteInt(-1);
            AssertBytesWritten(2);
            
            serializer.Writer.WriteInt(-150);
            AssertBytesWritten(3);
            
            serializer.Writer.WriteInt(-30000);
            AssertBytesWritten(3);
            
            serializer.Writer.WriteInt(-33000);
            AssertBytesWritten(5);
            
            serializer.Writer.WriteInt(1);
            AssertBytesWritten(2);
            
            serializer.Writer.WriteInt(150);
            AssertBytesWritten(3);
            
            serializer.Writer.WriteInt(30000);
            AssertBytesWritten(3);
            
            serializer.Writer.WriteInt(33000);
            AssertBytesWritten(5);
            
            // Message end
            serializer.Writer.Internal.WriteDataUInt(Bms1Tag.MessageEnd, 0);
            AssertBytesWritten(1);
            
            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            serializer.Reader.Internal.ReadAttributes();
            byte dataByte = 0;
            Assert.IsTrue(serializer.Reader.ReadByte(ref dataByte));
            Assert.AreEqual(234, dataByte);
            Assert.IsTrue(serializer.Reader.ReadByte(ref dataByte));
            Assert.AreEqual(0, dataByte);
            
            // Signed integer
            int dataInt = 2;
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(0, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-1, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-150, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-30000, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(-33000, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(1, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(150, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(30000, dataInt);
            Assert.IsTrue(serializer.Reader.ReadInt(ref dataInt));
            Assert.AreEqual(33000, dataInt);
            
            // Message end
            Assert.IsTrue(serializer.Reader.Internal.EndOfBlock);
        }
    }
}

