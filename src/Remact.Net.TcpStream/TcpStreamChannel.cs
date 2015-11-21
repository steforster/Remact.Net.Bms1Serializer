using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace Remact.Net.TcpStream
{
    /// <summary>
    /// TcpStreamChannel is used for incoming and outgoing message streams on client as well as on service side.
    /// The streams are optimized for serialization performance.
    /// Data is transferred using SocketAsyncEventArgs. Buffers are reused.
    /// The largest outgoing message defines the memory used for the output buffer.
    /// Incoming messages use a buffer of fixed size. Larger incoming messages use a threadpool thread to deserialize the message.
    /// </summary>
    public class TcpStreamChannel : IDisposable
    {
        private Action<TcpStreamChannel> _onDataReceivedAction;
        private Action<TcpStreamChannel> _onChannelDisconnectedAction;
        private int _bufferSize;

        private SocketAsyncEventArgs _receiveEventArgs;
        private TcpStreamIncoming _tcpStreamIncoming;
        private bool _userReadsMessage;

        private SocketAsyncEventArgs _sendEventArgs;
        private TcpStreamOutgoing _tcpStreamOutgoing;

        public Socket ClientSocket { get; private set; }

        /// <summary>
        /// Creates a buffered TCP stream.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public TcpStreamChannel(Socket clientSocket)
        {
            if (clientSocket == null)
            {
                throw new ArgumentNullException("clientSocket");
            }
            ClientSocket = clientSocket;
        }

        /// <summary>
        /// Starts the IO streams using the given socket. The socket must have been opened by a service or a client.
        /// </summary>
        /// <param name="onDataReceived">The action is called back (on a threadpool thread), when a message start is received.</param>
        /// <param name="onChannelDisconnected">The action is called back, when the channel is disconnected from remote. May be null.</param>
        /// <param name="bufferSize">Defines size of the receive buffer and minimum size of the send buffer. Default = 1500 bytes (one ethernet frame).</param>
        public void Start(Action<TcpStreamChannel> onDataReceived, Action<TcpStreamChannel> onChannelDisconnected = null, int bufferSize=1500)
        {
            if (onDataReceived == null)
            {
                throw new ArgumentNullException("onDataReceived");
            }

            if (_disposed || !ClientSocket.Connected)
            {
                DisposeSocket(ClientSocket);
                throw new SocketException((int)SocketError.NotConnected);
            }

            if (_onDataReceivedAction != null)
            {
                throw new InvalidOperationException("already started");
            }

            _onDataReceivedAction = onDataReceived;
            _onChannelDisconnectedAction = onChannelDisconnected;
            _bufferSize = bufferSize;

            _receiveEventArgs = new SocketAsyncEventArgs();
            _receiveEventArgs.SetBuffer(new byte[_bufferSize], 0, _bufferSize);
            _receiveEventArgs.Completed += OnDataReceived;
            _tcpStreamIncoming = new TcpStreamIncoming(StartAsyncRead);
            _userReadsMessage = false;

            _sendEventArgs = new SocketAsyncEventArgs();
            _sendEventArgs.Completed += OnDataSent;
            _tcpStreamOutgoing = new TcpStreamOutgoing(SendAsync, _bufferSize);

            StartAsyncRead();
        }

        /// <summary>
        /// A stream to deserialize incoming messages. Use it when your onDataReceived method is called back.
        /// </summary>
        public Stream InputStream  { get {return _tcpStreamIncoming; }}

        /// <summary>
        /// A stream to serialize outgoing messages. Call FlushAsync() to write the message buffer to the network.
        /// </summary>
        public TcpStreamOutgoing OutputStream { get { return _tcpStreamOutgoing; } }

        /// <summary>
        /// Returns true, when socket is connected. False otherwise.
        /// </summary>
        public bool IsConnected { get { return !_disposed && ClientSocket.Connected; } }

        /// <summary>
        /// The latest exception. Is null as long as client socket is connected.
        /// </summary>/
        public Exception LatestException { get; internal set; }


        // Starts the ReceiveEventArgs for next incoming data on server- or client side. Does not block.
        // Returns false, when socket is closed.
        // Examples: http://msdn.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs%28v=vs.110%29.aspx
        //           http://www.codeproject.com/Articles/22918/How-To-Use-the-SocketAsyncEventArgs-Class
        //           http://netrsc.blogspot.ch/2010/05/async-socket-server-sample-in-c.html
        private bool StartAsyncRead()
        {
            try
            {
                if (!_disposed)
                {
                    // when data arrives asynchronous, fill it into the buffer (see SetBuffer) and call OnDataReceived:
                    if (!ClientSocket.ReceiveAsync(_receiveEventArgs))
                    {
                        OnDataReceived(null, _receiveEventArgs); // data received synchronous
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnSurpriseDisconnect(ex, null);
            }
            return false;
        }

        // OnDataReceived is asynchronously called on a threadpool thread. Sending is on another thread.
        // Only one ReceiveEventArgs exist for one stream. Therefore, no concurrency while receiving.
        // At least under Mono 2.10.8 there is a threading issue (multi core ?) that can be prevented,
        // when we thread-lock access to the ReceiveAsync method.
        private void OnDataReceived(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                do
                {
                    int receivedBytes = e.BytesTransferred;
                    if (e.SocketError != SocketError.Success || receivedBytes <= 0)
                    {
                        OnSurpriseDisconnect(new IOException("socket error (" + e.SocketError.ToString() + ") when receiving from " + e.RemoteEndPoint), null);
                        return;
                    }

                    if (_userReadsMessage)
                    {
                        // Free the threadpool thread that is reading the message (on another thread).
                        // When even more data must be read, StartAsyncRead is called again by _tcpStreamIncoming.
                        _tcpStreamIncoming.DataReceived(e.Buffer, e.BytesTransferred, true);
                        return;
                    }

                    // set up the stream with the first bytes of a new message 
                    _tcpStreamIncoming.DataReceived(e.Buffer, e.BytesTransferred, false);

                    // Signal the user about an incoming message.
                    // This threadpool thread will deserialize the message using _tcpStreamIncoming.
                    // It may block, when more data must be awaited.
                    _userReadsMessage = true;
                    _onDataReceivedAction(this);
                    _userReadsMessage = false;
                }
                while (!_disposed && !ClientSocket.ReceiveAsync(e)); // returns false, when new data has been received synchronously
            }
            catch (Exception ex)
            {
                OnSurpriseDisconnect(ex, null);
            }
            //}
            //catch (OperationCanceledException) { }
        }


        // called from _tcpStreamOutgoing on user actor worker thread, when message has been serialized into buffer.
        private Task SendAsync(byte[] messageBuffer, int count)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                if (!_disposed)
                {
                    _sendEventArgs.SetBuffer(messageBuffer, 0, count);
                    _sendEventArgs.UserToken = tcs;
                    // when data is sent, call OnDataFlushed:
                    if (!ClientSocket.SendAsync(_sendEventArgs))
                    {
                        OnDataSent(null, _sendEventArgs);
                    }
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            }
            catch (Exception ex)
            {
                OnSurpriseDisconnect(ex, tcs);
            }

            return tcs.Task;
        }

        // asynchronously called on a threadpool thread:
        private void OnDataSent(object sender, SocketAsyncEventArgs e)
        {
            var tcs = (TaskCompletionSource<bool>)e.UserToken;
            try
            {
                if (e.SocketError == SocketError.Success && e.BytesTransferred == e.Count)
                {
                    int currentCount = (int)_tcpStreamOutgoing.Position;
                    _tcpStreamOutgoing.Position = 0;
                    if (e.Count == currentCount)
                    {
                        tcs.TrySetResult(true);
                    }
                    else
                    {
                        tcs.TrySetException(new InvalidOperationException("changed stream position from "+ e.Count + " to "+ currentCount + " when asynchronously sending to " + e.RemoteEndPoint));
                    }
                }
                else
                {
                    OnSurpriseDisconnect(new IOException("socket error (" + e.SocketError.ToString() + ") when sending to " + e.RemoteEndPoint), tcs);
                }
            }
            catch (Exception ex)
            {
                OnSurpriseDisconnect(ex, tcs);
            }
        }

        private void OnSurpriseDisconnect(Exception exception, TaskCompletionSource<bool> tcs)
        {
            Dispose();
            LatestException = exception;
            if (tcs != null)
            {
                tcs.TrySetException(exception);
            }

            if (_onChannelDisconnectedAction != null)
            {
                var callback = _onChannelDisconnectedAction;
                _onChannelDisconnectedAction = null;
                callback(this);
            }
        }


        #region IDisposable

        protected bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _userReadsMessage = false;
            if (disposing && !_disposed)
            {
                _disposed = true;
                if (_receiveEventArgs != null)
                {
                    _receiveEventArgs.Dispose();
                }

                if (_sendEventArgs != null)
                {
                    _sendEventArgs.Dispose();
                }

                if (ClientSocket != null && ClientSocket.Connected)
                {
                    try
                    {
                        ClientSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception shutdownException)
                    {
                        LatestException = shutdownException;
                    }

                    try
                    {
                        DisposeSocket(ClientSocket);
                    }
                    catch (Exception closeException)
                    {
                        LatestException = closeException;
                    }

                    ClientSocket = null;
                }

                if (_tcpStreamIncoming != null)
                {
                    _tcpStreamIncoming.Dispose();
                    _tcpStreamIncoming = null;
                }

                if (_tcpStreamOutgoing != null)
                {
                    _tcpStreamOutgoing.Dispose();
                    _tcpStreamOutgoing = null;
                }
            }
        }

        internal static void DisposeSocket(Socket socket)
        {
#if (!NET35)
            socket.Dispose();
#else
            socket.Close();
#endif
        }

        #endregion
    }
}
