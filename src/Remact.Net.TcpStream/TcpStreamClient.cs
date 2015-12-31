using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// Represents a TCP client.
    /// It is derived from TcpStreamChannel. The channel can be used after connecting to a remote TCP service.
    /// </summary>
    public class TcpStreamClient : TcpStreamChannel
    {
        private TaskCompletionSource<bool> _connectTcs;

        /// <summary>
        /// Initializes a new TcpStreamClient.
        /// </summary>
        public TcpStreamClient()
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) // TcpClient(AddressFamily.InterNetwork);
        {
            ClientSocket.LingerState.Enabled = false;
            _receiveEventArgs.Completed += OnClientConnected;
        }

        /// <summary>
        /// Connects to service and start communication.
        /// After a timeout defined by the operating system, the task fails when not connected.
        /// </summary>
        /// <param name="onDataReceived">The action is called back (on a threadpool thread), when a message start is received.</param>
        /// <param name="onChannelDisconnected">The action is called back (on a threadpool thread), when the channel is disconnected from remote. May be null.</param>
        /// <param name="bufferSize">Defines size of the receive buffer and minimum size of the send buffer. Default = 1500 bytes (one ethernet frame).</param>
        public async Task ConnectAsync(Uri uri, Action<TcpStreamChannel> onDataReceived, Action<TcpStreamChannel> onChannelDisconnected = null, int bufferSize = 1500)
        {
            await ConnectAsync(uri.Host, uri.Port, onDataReceived, onChannelDisconnected, bufferSize);
        }

        /// <summary>
        /// Connects to service and start communication.
        /// After a timeout defined by the operating system, the task fails when not connected.
        /// </summary>
        /// <param name="onDataReceived">The action is called back (on a threadpool thread), when a message start is received.</param>
        /// <param name="onChannelDisconnected">The action is called back (on a threadpool thread), when the channel is disconnected from remote. May be null.</param>
        /// <param name="bufferSize">Defines size of the receive buffer and minimum size of the send buffer. Default = 1500 bytes (one ethernet frame).</param>
        public async Task ConnectAsync(string hostOrIp, int tcpPort, Action<TcpStreamChannel> onDataReceived, Action<TcpStreamChannel> onChannelDisconnected = null, int bufferSize=1500)
        {
            await ConnectAsync(hostOrIp, tcpPort);
            Start(onDataReceived, onChannelDisconnected, bufferSize);
        }

        /// <summary>
        /// Connects to a service. After a timeout defined by the operating system, the task fails when not connected.
        /// After ConnectAsync has executed successfully, the <see cref="TcpStreamChannel.Start" /> method must be called.
        /// </summary>
        /// <param name="hostOrIp">The host name or ip address of the remote service.</param>
        /// <param name="tcpPort">The TCP port of the remote service.</param>
        public async Task ConnectAsync(string hostOrIp, int tcpPort)
        {
//            IPAddress ipAddress;
//            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
//            {
//            }

            await ConnectAsync(new DnsEndPoint(hostOrIp, tcpPort));
        }

        /// <summary>
        /// Connects to a service. After a timeout defined by the operating system, the task fails when not connected.
        /// After ConnectAsync has executed successfully, the <see cref="TcpStreamChannel.Start" /> method must be called.
        /// </summary>
        /// <param name="remoteEndpoint">The endpoint address of the remote service.</param>
        public async Task ConnectAsync(EndPoint remoteEndpoint)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Remact.Net.TcpStream.TcpStreamClient"); // in VS2015: nameof(TcpStreamClient));
            }
            if (IsConnected)
            {
                throw new InvalidOperationException("cannot restart ConnectAsync, already connected");
            }
            if (_connectTcs != null)
            {
                throw new InvalidOperationException("cannot restart ConnectAsync, operation in progress");
            }

            RemoteEndPoint = remoteEndpoint;
            _receiveEventArgs.RemoteEndPoint = remoteEndpoint;
            _connectTcs = new TaskCompletionSource<bool>();
            var task = _connectTcs.Task;

            if (!ClientSocket.ConnectAsync(_receiveEventArgs))
            {
                OnClientConnected(null, _receiveEventArgs);
            }
            await task;
        }


        /// <summary>
        /// A client has connected. Called from socket on threadpool thread.
        /// </summary>
        private void OnClientConnected(object sender, SocketAsyncEventArgs e)
        {
            var tcs = _connectTcs;
            _connectTcs = null;
            try
            {
                if (!_disposed && e.SocketError == SocketError.Success && ClientSocket.Connected)
                {
                    _receiveEventArgs.Completed -= OnClientConnected;
                    tcs.TrySetResult(true);
                }
                else
                {
                    LatestException = new IOException("socket error (" + e.SocketError.ToString() + ") when connecting to " + RemoteEndPoint);
                    tcs.TrySetException(LatestException);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LatestException = ex;
                tcs.TrySetException(LatestException);
            }
        }

        
        #region IDisposable

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
