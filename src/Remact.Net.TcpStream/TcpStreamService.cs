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
        private IPEndPoint _endPoint;
        private ManualResetEvent _listenResetEvent = new ManualResetEvent(false);
        private SocketAsyncEventArgs _acceptEventArg;
        private Action<TcpStreamChannel> _onClientAcceptedAction;


        /// <summary>
        /// Constructs an instance of the service and starts listening for incoming connections on any ip address.
        /// </summary>
        /// <param name="port">The port number for connect requests.</param>
        /// <param name="onClientConnected">The callback is fired, when a client connects to the service.</param>
        public TcpStreamService(int port, Action<TcpStreamChannel> onClientConnected)
        {
            Initialize(new IPEndPoint(IPAddress.Any, port), onClientConnected);
        }

        /// <summary>
        /// Constructs an instance of the service and starts listening for incoming connections on designated endpoint.
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
            _endPoint = endpoint;
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            _listenSocket.Bind(_endPoint);
            _listenSocket.Listen(10);

            _acceptEventArg = new SocketAsyncEventArgs();
            _acceptEventArg.Completed += OnAcceptNewClient;

        }

        /// <summary>
        /// Gets the end point this service is listening on.
        /// </summary>
        public IPEndPoint ListeningEndPoint
        {
            get { return _endPoint; }
        }

        /// <summary>
        /// The latest exception. Is null as long as the listener socket is running.
        /// </summary>/
        public Exception LatestException { get; private set; }


        /// <summary>
        /// Start listening for next incoming TCP connection request.
        /// </summary>
        private void StartAsyncListening()
        {
            if (!_disposed && !_listenSocket.AcceptAsync(_acceptEventArg))
            {
                OnAcceptNewClient(null, _acceptEventArg);
            }
        }

        /// <summary>
        /// A client has connected. Called from socket on threadpool thread.
        /// </summary>
        private void OnAcceptNewClient(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (_disposed || e.SocketError != SocketError.Success)
                {
                    LatestException = new SocketException((int)e.SocketError);
                    return;
                }

                var channel = new TcpStreamChannel(e.AcceptSocket);
                e.AcceptSocket = null;

                _onClientAcceptedAction(channel); // Callback to user. User has to start the channel.

                StartAsyncListening();
            }
            catch (Exception ex)
            {
                Dispose();
                LatestException = ex;
            }
        }


        #region IDisposable Members

        private bool _disposed;

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
