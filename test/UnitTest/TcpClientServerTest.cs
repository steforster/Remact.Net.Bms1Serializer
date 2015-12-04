using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Remact.Net.TcpStream;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Remact.Net.Bms1UnitTest
{
    using System.Diagnostics;
    using System.Runtime.Remoting.Services;

    [TestClass]
    [TestFixture]
    public class TcpClientServerTest
    {
        TcpStreamService _service;
        TcpStreamChannel _stub1;
        TcpStreamChannel _stub2;
        ManualResetEventSlim _serviceConnectedEvent = new ManualResetEventSlim();
        ManualResetEventSlim _clientReceivedEvent = new ManualResetEventSlim();

        int NormalTimeout
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return 60 * 60 * 1000;
                }
                return 100;
            }
        }

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

            channel.Start(OnDataReceivedByService, OnServiceChannelDisconnected);
            _serviceConnectedEvent.Set();
        }

        void OnDataReceivedByService(TcpStreamChannel channel)
        {
            while (channel.IsConnected)
            {
                var received = channel.InputStream.ReadByte();
                if (received >= 0)
                {
                    channel.OutputStream.WriteByte((byte)received);
                }

                if (channel.InputStream.Length == 0)
                {
                    channel.OutputStream.Flush();
                }
            }
        }

        void OnServiceChannelDisconnected(TcpStreamChannel channel)
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
                _serviceConnectedEvent.Wait(NormalTimeout);
                Assert.IsTrue(_stub1.IsConnected);
                Assert.IsNull(_stub2);

                _serviceConnectedEvent.Reset();
                await client2.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port);
                Assert.IsTrue(client1.IsConnected);
                Assert.IsTrue(client2.IsConnected);
                Assert.IsTrue(_stub1.IsConnected);
                _serviceConnectedEvent.Wait(NormalTimeout);
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



        private decimal _receivedDec;
        private int _receiveDelay;

        void OnDataReceivedByClient(TcpStreamChannel channel)
        {
            Thread.Sleep(_receiveDelay);
            var reader = new BinaryReader(channel.InputStream);
            _receivedDec = reader.ReadDecimal();
            _clientReceivedEvent.Set();
        }

        void OnClientChannelDisconnected(TcpStreamChannel channel)
        {

        }

        [TestMethod]
        [Test]
        public async Task TransferReceiveFast()
        {
            using(var client1 = new TcpStreamClient())
            {
                _clientReceivedEvent.Reset();
                _receiveDelay = 0;
                await client1.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port, OnDataReceivedByClient);
                var writer = new BinaryWriter(client1.OutputStream);
                var sent = 1234567890.123456789m;
                writer.Write(sent);
                await client1.OutputStream.FlushAsync();
                Assert.IsTrue(_clientReceivedEvent.Wait(NormalTimeout));
                Assert.AreEqual(sent, _receivedDec);
            }
        }

        [TestMethod]
        [Test]
        public async Task TransferSendFast()
        {
            using (var client1 = new TcpStreamClient())
            {
                _clientReceivedEvent.Reset();
                _receiveDelay = 50;
                await client1.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port, OnDataReceivedByClient, OnClientChannelDisconnected);
                var writer = new BinaryWriter(client1.OutputStream);
                var sent = 1234567890.987654321m;
                writer.Write(sent);
                client1.OutputStream.Flush(); // synchronous
                Assert.IsTrue(_clientReceivedEvent.Wait(NormalTimeout));
                Assert.AreEqual(sent, _receivedDec);
            }
        }


        [TestMethod]
        [Test]
        public async Task TransferLargeArray()
        {
            using (var client1 = new TcpStreamClient())
            {
                _clientReceivedEvent.Reset();
                _receivedBytes = null;
                _expectedCount = 100000;
                await client1.ConnectAsync("127.0.0.1", _service.ListeningEndPoint.Port);
                client1.Start(OnLargeArrayReceivedByClient);

                var writer = new BinaryWriter(client1.OutputStream);

                var sent = 0;
                while (sent < _expectedCount)
                {
                    int n = System.Math.Min(sent + 1, _expectedCount - sent);
                    var buf = new byte[n];
                    for (int i = 0; i < n; i++)
                    {
                        buf[i] = (byte)sent++;
                    }

                    writer.Write(buf);
                    client1.OutputStream.Flush(); // synchronous
                    Thread.Sleep(10);
                }

                Assert.IsTrue(_clientReceivedEvent.Wait(NormalTimeout));
                Assert.IsNotNull(_receivedBytes);
                Assert.AreEqual(_expectedCount, _receivedBytes.Length);
                for (int i = 0; i < _expectedCount; i++)
                {
                    if (_receivedBytes[i] != (byte)i)
                    {
                        Assert.Fail("received {0} at index {1}", _receivedBytes[i], i);
                    }
                }
            }
        }

        private byte[] _receivedBytes;
        private int _expectedCount;

        void OnLargeArrayReceivedByClient(TcpStreamChannel channel)
        {
            var reader = new BinaryReader(channel.InputStream);
            _receivedBytes = reader.ReadBytes(_expectedCount);
            _clientReceivedEvent.Set();
        }
    }
}

