namespace Remact.Net.Bms1Serializer.UnitTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NUnit.Framework;
    using System.IO;

    [TestClass]
    [TestFixture]
    public class MessageTest
    {
        MemoryStream _stream;
        int _latestPosition;
        Bms1MessageSerializer _serializer;
        
        [TestInitialize]
        [SetUp]
        public void TestInitialize()
        {
            _stream = new MemoryStream();
            _serializer = new Bms1MessageSerializer();
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
        public void SerializeIdleMessage()
        {
            // Write data -------------------------------------
            var request = new IdleMessage();
            _serializer.WriteMessage(_stream, request);
            AssertBytesWritten(10);
            

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            Assert.AreEqual(1, _serializer.ReadMessageStart(_stream));
            var reply = _serializer.ReadMessage(IdleMessage.ReadFromBms1Stream);
            Assert.IsNotNull(reply);
            Assert.AreEqual(10, _stream.Position);
        }
    }
}

