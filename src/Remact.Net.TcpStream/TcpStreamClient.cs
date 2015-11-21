using System;
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
        private Timer _timer;

        public TcpStreamClient()
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) // TcpClient(AddressFamily.InterNetwork);
        {
            ClientSocket.LingerState.Enabled = false;
            _connectEventArgs = new SocketAsyncEventArgs();
            _connectEventArgs.Completed += OnClientConnected;
            _timer = new Timer(OnTimeoutExpired);
        }

        /// <summary>
        /// After successfully connected, the Start method must be called (see TcpStreamChannel).
        /// </summary>
        /// <param name="remoteEndpoint"></param>
        public Task ConnectAsync(IPEndPoint remoteEndpoint, int connectTimeoutMs)
        {
            var tcs = new TaskCompletionSource<bool>();
            _connectEventArgs.RemoteEndPoint = remoteEndpoint;
            _connectEventArgs.UserToken = tcs;
            _timer.Change(connectTimeoutMs, Timeout.Infinite);

            if (!ClientSocket.ConnectAsync(_connectEventArgs))
            {
                OnClientConnected(null, _connectEventArgs);
            }
            return tcs.Task;
        }


        /// <summary>
        /// A client has connected. Called from socket on threadpool thread.
        /// </summary>
        private void OnClientConnected(object sender, SocketAsyncEventArgs e)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            var tcs = (TaskCompletionSource<bool>)e.UserToken;
            try
            {
                if (!_disposed && e.SocketError == SocketError.Success)
                {
                    tcs.TrySetResult(true);
                }
                else
                {
                    LatestException = new IOException("socket error (" + e.SocketError.ToString() + ") when connecting to " + _connectEventArgs.RemoteEndPoint);
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

        private void OnTimeoutExpired(object state)
        {
            var tcs = (TaskCompletionSource<bool>)state;
            var ex = new IOException("timeout when connecting to "+ _connectEventArgs.RemoteEndPoint);
            if (tcs.TrySetException(LatestException))
            {
                LatestException = ex;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _connectEventArgs != null)
            {
                _connectEventArgs.Dispose();
                _connectEventArgs = null;
            }
            base.Dispose(disposing);
        }
    }
}
