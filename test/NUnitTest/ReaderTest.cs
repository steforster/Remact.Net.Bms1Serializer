namespace Remact.Net.Bms1Serializer.NUnitTest
{
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class ReaderTest
    {
        [Test]
        public void ReadSimpleTypes()
        {
            var serializer = new Bms1MessageSerializer();
            var stream = new MemoryStream();
            serializer.WriteMessage(stream, 0, null);
            serializer.Writer.WriteByte(123);
            
            var msgId = serializer.ReadMessageStart(stream);
            serializer.Reader.Internal.ReadAttributes(); // skip block start
            byte dataByte = 0;
            Assert.IsTrue(serializer.Reader.ReadByte(ref dataByte));
            Assert.AreEqual(123, dataByte);
        }
    }
}

