using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Remact.Net.TcpStream;

namespace Remact.Net.Bms1UnitTest
{

    [TestClass]
    [TestFixture]
    public class TcpClientServerTest
    {
        TcpStreamService _service;
        TcpStreamChannel _stub1;
        TcpStreamChannel _stub2;
        TcpStreamClient _client;
        
        [TestInitialize]
        [SetUp]
        public void TestInitialize()
        {
            _stub1 = null;
            _stub2 = null;
            _service = new TcpStreamService(0, OnClientAcceptedByService);
            _client = new TcpStreamClient();
        }

        [TestCleanup]
        [TearDown]
        public void TestCleanup()
        {
            if (_stub1 != null)
            {
                _stub1.Dispose();
            }

            if (_stub2 != null)
            {
                _stub2.Dispose();
            }

            _service.Dispose();
            _client.Dispose();
        }



        void OnClientAcceptedByService(TcpStreamChannel channel)
        {
            ;
        }

        
        [TestMethod]
        [Test]
        public void OpenService()
        {
        }


        [TestMethod]
        [Test]
        public void ConnectClientUnsuccessful()
        {
        }


        [TestMethod]
        [Test]
        public void ConnectClientsSuccessful()
        {
        }


        [TestMethod]
        [Test]
        public void TransferReceiveFast()
        {
        }


        [TestMethod]
        [Test]
        public void TransferSendFast()
        {
        }


        [TestMethod]
        [Test]
        public void TransferLargeArray()
        {
        }
    }
}

