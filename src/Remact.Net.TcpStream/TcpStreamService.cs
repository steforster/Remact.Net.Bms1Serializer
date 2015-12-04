using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// Represents a TCP service that listens for clients to connect.
    /// </summary>
    public class TcpStreamService : IDisposable
    {
        private Socket _listenSocket;
        private ManualResetEvent _listenResetEvent = new ManualResetEvent(false);
        private SocketAsyncEventArgs _acceptEventArg;
        private Action<TcpStreamChannel> _onClientAcceptedAction;


        /// <summary>
        /// Constructs an instance of the service and starts listening for incoming connections on any local ethernet adapter.
        /// </summary>
        /// <param name="port">The port number for connect requests.</param>
        /// <param name="onClientAccepted">The callback is fired, when the service accepts a new client connection.</param>
        public TcpStreamService(int port, Action<TcpStreamChannel> onClientAccepted)
        {
            Initialize(new IPEndPoint(IPAddress.Any, port), onClientAccepted);
        }

        /// <summary>
        /// Constructs an instance of the service and starts listening for incoming connections on a designated ethernet adapter.
        /// </summary>
        /// <param name="endpoint">The ethernet adapter and port number for connect requests.</param>
        /// <param name="onClientAccepted">The callback is fired, when the service accepts a new client connection.</param>
        public TcpStreamService(IPEndPoint endpoint, Action<TcpStreamChannel> onClientAccepted)
        {
           Initialize(endpoint, onClientAccepted);
        }

        private void Initialize(IPEndPoint endpoint, Action<TcpStreamChannel> onClientAccepted)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }
            if (onClientAccepted == null)
            {
                throw new ArgumentNullException("onClientConnected");
            }

            _onClientAcceptedAction = onClientAccepted;
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _listenSocket.Bind(endpoint);
            _listenSocket.Listen(10);

            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += OnAcceptNewClient;

            if (!_listenSocket.AcceptAsync(_acceptEventArg))
            {
                OnAcceptNewClient(null, _acceptEventArg);
            }
        }

        /// <summary>
        /// Gets the end point this service is listening on. Returns null, when disposed.
        /// </summary>
        public IPEndPoint ListeningEndPoint
        {
            get { return _disposed ? null : _listenSocket.LocalEndPoint as IPEndPoint; }
        }

        /// <summary>
        /// The latest exception. Is null as long as the listener socket is running.
        /// </summary>/
        public Exception LatestException { get; private set; }


        /// <summary>
        /// A client has connected. Called from socket on threadpool thread.
        /// </summary>
        private void OnAcceptNewClient(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                do
                {
                    if (_disposed || e.SocketError != SocketError.Success)
                    {
                        LatestException = new SocketException((int)e.SocketError);
                        return;
                    }

                    var channel = new TcpStreamChannel(e.AcceptSocket);
                    e.AcceptSocket = null;

                    // Callback to user. User has to start the new channel.
                    _onClientAcceptedAction(channel); 
                }
                while (!_disposed && !_listenSocket.AcceptAsync(_acceptEventArg));
            }
            catch (Exception ex)
            {
                Dispose();
                LatestException = ex;
            }
        }


        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Disposes the underlying listener socket and all reserved resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _listenResetEvent.Set();
                    _acceptEventArg.Dispose();
                    TcpStreamChannel.DisposeSocket(_listenSocket);
                    _listenResetEvent.Close();
                }
            }
        }

        #endregion
    }
}
