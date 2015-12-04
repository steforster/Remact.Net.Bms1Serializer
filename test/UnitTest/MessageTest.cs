using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Remact.Net.Bms1Serializer;

namespace Remact.Net.Bms1UnitTest
{
    using System;

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
        public void SendIdleMessage()
        {
            // Write data -------------------------------------
            var request = new IdleMessage();
            _serializer.WriteMessage(_stream, request.WriteToBms1Stream);
            AssertBytesWritten(10);
            

            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            Assert.AreEqual(1, _serializer.ReadMessageStart(_stream));
            var reply = _serializer.ReadMessage(IdleMessage.ReadFromBms1Stream);
            Assert.IsNotNull(reply);
            Assert.AreEqual(10, _stream.Position);
        }

        [TestMethod]
        [Test]
        public void SendIdentificationMessageDotNet()
        {
            // Write data -------------------------------------
            var request = new IdentificationMessage
            {
                ApplicationInstance = "123",
                ApplicationName = "MyApp",
                InterfaceName = "MyInterface",
                InterfaceVersion = 123,
                ApplicationVersion = new VersionDotNet { Version = new Version(1, 2, 3, 4) }
            };

            _serializer.WriteMessage(_stream, request.WriteToBms1Stream);
            AssertBytesWritten(51);


            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            Assert.AreEqual(2, _serializer.ReadMessageStart(_stream));
            var reply = _serializer.ReadMessage(IdentificationMessage.ReadFromBms1Stream);
            Assert.IsNotNull(reply);
            AssertEqual(request, reply);
            Assert.AreEqual(51, _stream.Position);
        }


        [TestMethod]
        [Test]
        public void SendIdentificationMessagePLC()
        {
            // Write data -------------------------------------
            var request = new IdentificationMessage
            {
                ApplicationInstance = "1234",
                ApplicationName = "MyApp2",
                InterfaceName = "MyInterface2",
                InterfaceVersion = 1234,
                ApplicationVersion = new VersionPLC { Version = "1.2.3.4", CpuType = CpuType.ArmCortexA5, AdditionaInfo = "more info" }
            };

            _serializer.WriteMessage(_stream, request.WriteToBms1Stream);
            AssertBytesWritten(56);


            // Read and verify data ----------------------------------------
            _stream.Position = 0;
            Assert.AreEqual(2, _serializer.ReadMessageStart(_stream));
            var reply = _serializer.ReadMessage(IdentificationMessage.ReadFromBms1Stream);
            Assert.IsNotNull(reply);
            (request.ApplicationVersion as VersionPLC).AdditionaInfo = "None"; // the AdditionaInfo is not written to stream
            AssertEqual(request, reply);
            Assert.AreEqual(56, _stream.Position);
        }

        public void AssertEqual(IdentificationMessage m1, IdentificationMessage m2)
        {
            Assert.AreEqual(m1.ApplicationInstance, m2.ApplicationInstance);
            Assert.AreEqual(m1.ApplicationName, m2.ApplicationName);
            Assert.AreEqual(m1.InterfaceName, m2.InterfaceName);
            Assert.AreEqual(m1.InterfaceVersion, m2.InterfaceVersion);

            var d1 = m1.ApplicationVersion as VersionDotNet;
            if (d1 != null)
            {
                var d2 = m2.ApplicationVersion as VersionDotNet;
                Assert.AreEqual(d1.Version, d2.Version);
            }
            else
            {
                var p1 = m1.ApplicationVersion as VersionPLC;
                var p2 = m2.ApplicationVersion as VersionPLC;
                Assert.AreEqual(p1.Version, p2.Version);
                Assert.AreEqual(p1.CpuType, p2.CpuType);
                Assert.AreEqual(p1.AdditionaInfo, p2.AdditionaInfo);
            }
        }
    }
}

