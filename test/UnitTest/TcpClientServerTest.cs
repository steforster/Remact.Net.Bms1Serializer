using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Remact.Net.TcpStream;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Remact.Net.Bms1UnitTest
{

    [TestClass]
    [TestFixture]
    public class TcpClientServerTest
    {
        TcpStreamService _service;
        TcpStreamChannel _stub1;
        TcpStreamChannel _stub2;
        ManualResetEventSlim _serviceConnectedEvent = new ManualResetEventSlim();

        [TestInitialize]
        [SetUp]
        public void TestInitialize()
        {
            _stub1 = null;
            _stub2 = null;
            _service = new TcpStreamService(0, OnClientAcceptedByService);
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
        }

        void OnClientAcceptedByService(TcpStreamChannel channel)
        {
            if (_stub1 == null)
            {
                _stub1 = channel;
            }
            else if (_stub2 == null)
            {
                _stub2 = channel;
            }
            else
            {
                Assert.Fail("too many accepted clients");
            }

            channel.Start(OnDataReceived, OnChannelDisconnected);
            _serviceConnectedEvent.Set();
        }

        void OnDataReceived(TcpStreamChannel channel)
        {

        }

        void OnChannelDisconnected(TcpStreamChannel channel)
        {

        }



        [TestMethod]
        [Test]
        public async Task OpenService()
        {
            Assert.IsNotNull(_service.ListeningEndPoint);
            Assert.IsTrue(_service.ListeningEndPoint.Port > 0);
            Assert.IsNull(_service.LatestException);

            _service.Dispose();
            Assert.IsNull(_service.ListeningEndPoint);
            await Task.Delay(10);
            Assert.IsNotNull(_service.LatestException);
        }


        [TestMethod]
        [Test]
        public async Task ConnectClientsSuccessful()
        {
            var client1 = new TcpStreamClient();
            var client2 = new TcpStreamClient();
            try
            {
                Assert.IsFalse(client1.IsConnected);
                Assert.IsFalse(client2.IsConnected);
                Assert.IsNull(_stub1);
                Assert.IsNull(_stub2);

                _serviceConnectedEvent.Reset();
                await client1.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port);
                Assert.IsTrue(client1.IsConnected);
                _serviceConnectedEvent.Wait();
                Assert.IsTrue(_stub1.IsConnected);
                Assert.IsNull(_stub2);

                _serviceConnectedEvent.Reset();
                await client2.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port);
                Assert.IsTrue(client1.IsConnected);
                Assert.IsTrue(client2.IsConnected);
                Assert.IsTrue(_stub1.IsConnected);
                _serviceConnectedEvent.Wait();
                Assert.IsTrue(_stub2.IsConnected);
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }


        [TestMethod]
        [Test]
        public async Task ConnectClientUnsuccessful()
        {
            var client1 = new TcpStreamClient();
            try
            {
                Assert.IsNull(client1.RemoteEndPoint);
                _serviceConnectedEvent.Reset();
                try
                {
                    await client1.ConnectAsync("127.0.0.1", 9000);
                }
                catch (IOException)
                {
                    Assert.IsNotNull(client1.RemoteEndPoint);
                    Assert.IsFalse(client1.IsConnected);
                    _serviceConnectedEvent.Wait(100);
                    Assert.IsNull(_stub1);
                    Assert.IsNull(_stub2);
                    return;
                }
                Assert.Fail("expected IOException");
            }
            finally
            {
                client1.Dispose();
            }
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

