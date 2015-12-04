using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// Represents a TCP client.
    /// It is derived from TcpStreamChannel. The channel can be used after connecting to a remote TCP service.
    /// </summary>
    public class TcpStreamClient : TcpStreamChannel
    {
        private SocketAsyncEventArgs _connectEventArgs;
        private TaskCompletionSource<bool> _connectTcs;

        /// <summary>
        /// Initializes a new TcpStreamClient.
        /// </summary>
        public TcpStreamClient()
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) // TcpClient(AddressFamily.InterNetwork);
        {
            ClientSocket.LingerState.Enabled = false;
            _connectEventArgs = new SocketAsyncEventArgs();
            _connectEventArgs.Completed += OnClientConnected;
        }

        /// <summary>
        /// Connects to service and start communication.
        /// After a timeout defined by the operating system, the task fails when not connected.
        /// </summary>
        /// <param name="onDataReceived">The action is called back (on a threadpool thread), when a message start is received.</param>
        /// <param name="onChannelDisconnected">The action is called back (on a threadpool thread), when the channel is disconnected from remote. May be null.</param>
        /// <param name="bufferSize">Defines size of the receive buffer and minimum size of the send buffer. Default = 1500 bytes (one ethernet frame).</param>
        public Task ConnectAsync(Uri uri, Action<TcpStreamChannel> onDataReceived, Action<TcpStreamChannel> onChannelDisconnected = null, int bufferSize = 1500)
        {
            return ConnectAsync(uri.Host, uri.Port, onDataReceived, onChannelDisconnected, bufferSize);
        }

        /// <summary>
        /// Connects to service and start communication.
        /// After a timeout defined by the operating system, the task fails when not connected.
        /// </summary>
        /// <param name="onDataReceived">The action is called back (on a threadpool thread), when a message start is received.</param>
        /// <param name="onChannelDisconnected">The action is called back (on a threadpool thread), when the channel is disconnected from remote. May be null.</param>
        /// <param name="bufferSize">Defines size of the receive buffer and minimum size of the send buffer. Default = 1500 bytes (one ethernet frame).</param>
        public Task ConnectAsync(string hostOrIp, int tcpPort, Action<TcpStreamChannel> onDataReceived, Action<TcpStreamChannel> onChannelDisconnected = null, int bufferSize=1500)
        {
            var task = ConnectAsync(hostOrIp, tcpPort);
            return task.ContinueWith(delegate(Task t)
                {
                    Start(onDataReceived, onChannelDisconnected, bufferSize);
                    return t;
                });
        }

        /// <summary>
        /// Connects to a service. After a timeout defined by the operating system, the task fails when not connected.
        /// After ConnectAsync has executed successfully, the <see cref="TcpStreamChannel.Start" /> method must be called.
        /// </summary>
        /// <param name="hostOrIp">The host name or ip address of the remote service.</param>
        /// <param name="tcpPort">The TCP port of the remote service.</param>
        public Task ConnectAsync(string hostOrIp, int tcpPort)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(hostOrIp, out ipAddress))
            {
                // TODO
            }

            return ConnectAsync(new IPEndPoint(ipAddress, tcpPort));
        }

        /// <summary>
        /// Connects to a service. After a timeout defined by the operating system, the task fails when not connected.
        /// After ConnectAsync has executed successfully, the <see cref="TcpStreamChannel.Start" /> method must be called.
        /// </summary>
        /// <param name="remoteEndpoint">The endpoint address of the remote service.</param>
        public Task ConnectAsync(EndPoint remoteEndpoint)
        {
            RemoteEndPoint = remoteEndpoint;
            _connectEventArgs.RemoteEndPoint = remoteEndpoint;
            _connectTcs = new TaskCompletionSource<bool>();

            if (!ClientSocket.ConnectAsync(_connectEventArgs))
            {
                OnClientConnected(null, _connectEventArgs);
            }
            return _connectTcs.Task;
        }


        /// <summary>
        /// A client has connected. Called from socket on threadpool thread.
        /// </summary>
        private void OnClientConnected(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (!_disposed && e.SocketError == SocketError.Success && ClientSocket.Connected)
                {
                    _connectTcs.TrySetResult(true);
                }
                else
                {
                    LatestException = new IOException("socket error (" + e.SocketError.ToString() + ") when connecting to " + RemoteEndPoint);
                    _connectTcs.TrySetException(LatestException);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                LatestException = ex;
                _connectTcs.TrySetException(LatestException);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _connectEventArgs != null)
            {
                _connectEventArgs.Dispose();
                _connectEventArgs = null;
            }
        }
    }
}
