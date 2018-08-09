using MsgMgr.Core;
using MsgMgr.Serialization;
using MsgMgrCommon.Extensions;
using MsgMgrCommon.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MsgMgr.Connections
{
    public abstract class TcpConnection : IConnection
    {
        private IPEndPoint _ipEndPoint;
        private Socket _client;

        private MemoryStream _sendStream;
        private MemoryStream _receiveStream;
        private byte[] _sendBuffer;
        private byte[] _receiveBuffer;


        protected Socket Client
        {
            get
            {
                return _client;
            }

            set
            {
                _client = value;
            }
        }

        protected IPEndPoint IpEndPoint
        {
            get
            {
                return _ipEndPoint;
            }

            private set
            {
                _ipEndPoint = value;
            }
        }

        protected TcpConnection(string ipAddress, int portNumber, int bufferSize = 1024)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            IpEndPoint = new IPEndPoint(ip, portNumber);
            _sendBuffer = new byte[bufferSize];
            _receiveBuffer = new byte[bufferSize];
            _sendStream = new MemoryStream(_sendBuffer, 0, _sendBuffer.Length, true, true);
            _receiveStream = new MemoryStream(_receiveBuffer, 0, _receiveBuffer.Length, true, true);

             Client = null;
        }

        /// <summary>
        /// Initializes a new connection.  If a connection already exists that conflicts with this connection attempt, the IConnection
        /// implementation should disconnect the existing connection
        /// </summary>
        public abstract void InitializeNewConnection();

        /// <summary>
        /// Disconnects the IConnection if connected.  Implementation should be idempotent
        /// </summary>
        public void DisconnectIfConnected()
        {
            if (Client != null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        /// <summary>
        /// Performs a single send action, sending the specified message.  Return value indicates whether the IConnection is still connected
        /// </summary>
        /// <param name="toSend">Message to send.</param>
        /// <param name="serType">Type of the serialization to use.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">TcpServer not initialized</exception>
        /// <exception cref="InvalidOperationException">TcpServer not initialized</exception>
        public bool Send(MessageBase toSend, SerializationType serType)
        {
            if (Client == null) { throw new InvalidOperationException("TcpServer not initialized"); }

            byte[] data = toSend.Serialize(serType);
            int length = data.Length;

            _sendStream.Position = 0;
            byte[] serLen = length.Serialize();

            _sendStream.Write(serLen, 0, sizeof(int));

            int totalLength = length + sizeof(int);

            _sendStream.Write(data, 0, length);

            SocketError er;

            int sent = Client.Send(_sendStream.GetBuffer(), 0, totalLength, SocketFlags.None, out er);

            if(er == SocketError.Success)
            {
                return true;
            }
            else //if (er == SocketError.ConnectionAborted)
            {
                return false;
            }
        }

        /// <summary>
        /// Performs a single receive attempt.  Returns the resulting data from the receive, and the out parameter indicates whether the IConnection is still connected
        /// </summary>
        /// <param name="stillConnected">if set to <c>true</c> [still connected].</param>
        /// <param name="serType">Type of the serialization to use.</param>
        /// <returns>
        /// The received data
        /// </returns>
        /// <exception cref="System.InvalidOperationException">TcpServer not initialized</exception>
        /// <exception cref="InvalidOperationException">TcpServer not initialized</exception>
        public MessageBase Receive(out bool stillConnected, SerializationType serType)
        {
            if (Client == null) { throw new InvalidOperationException("TcpServer not initialized"); }

            stillConnected = true;
            int count = 0;
            byte[] lengthBuffer = new byte[sizeof(int)];
            SocketError err;
            int receivedLen = Client.Receive(lengthBuffer, 0, sizeof(int), SocketFlags.None, out err);
            byte[] result = null;
            if(err == SocketError.Success)
            {
                int bytesToRead = BitConverter.ToInt32(lengthBuffer, 0);

                if (bytesToRead == 0)
                {
                    Logger.Instance.LogMessage("Received zero bytes from endpoint", LogPriority.LOW, LogCategory.NETWORK);
                    stillConnected = false;
                }
                else
                {
                    result = new byte[bytesToRead];
                
                    while (bytesToRead > 0)
                    {
                        int read = Client.Receive(result, count, bytesToRead, SocketFlags.None, out err);

                        if(err == SocketError.Success)
                        {
                            bytesToRead -= read;
                            count += read;
                        }
                        else
                        {
                            stillConnected = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                stillConnected = false;
            }

            MessageBase toReturn;
            if(result != null)
            {
                toReturn = (MessageBase)SerializableBase.Deserialize(result, serType);
            }
            else
            {
                toReturn = null;
            }

            return toReturn;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisconnectIfConnected();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TcpClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {

            Logger.Instance.LogMessage("Disposing TcpConnection", LogPriority.HIGH, LogCategory.INFO);

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
