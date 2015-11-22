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
        /// After successfully connected, the Start method must be called (see TcpStreamChannel).
        /// After a timeout defined by the operating system, the task fails when not connected.
        /// </summary>
        public Task ConnectAsync(Uri uri)
        {
            return ConnectAsync(uri.Host, uri.Port);
        }

        /// <summary>
        /// After successfully connected, the Start method must be called (see TcpStreamChannel).
        /// </summary>
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
        /// After successfully connected, the Start method must be called (see TcpStreamChannel).
        /// </summary>
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
                if (!_disposed && e.SocketError == SocketError.Success)
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
