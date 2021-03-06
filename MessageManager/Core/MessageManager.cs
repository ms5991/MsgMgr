﻿using MsgMgr.Messages;
using MsgMgr.Serialization;
using MsgMgrCommon.Extensions;
using MsgMgrCommon.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MsgMgr.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class MessageManager : IDisposable
    {
        #region Fields

        /// <summary>
        /// The send queue
        /// </summary>
        private ConcurrentQueue<MessageBase> _sendQueue;

        /// <summary>
        /// The receiver that will process received messages
        /// </summary>
        private MessageReceiver _receiver;

        /// <summary>
        /// The connection
        /// </summary>
        private IConnection _connection;

        /// <summary>
        /// The connection
        /// </summary>
        private SerializationType _serializationType;

        /// <summary>
        /// The send receive token source
        /// </summary>
        private readonly CancellationTokenSource _sendReceiveTokenSource;

        /// <summary>
        /// The receive stream
        /// </summary>
        private readonly MemoryStream _receiveStream;

        /// <summary>
        /// The is managing
        /// </summary>
        private volatile bool _isManaging;

        /// <summary>
        /// The send task
        /// </summary>
        private Task _sendTask;

        /// <summary>
        /// The receive task
        /// </summary>
        private Task _receiveTask;

        private object _managingLock = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageManager"/> class.
        /// </summary>
        public MessageManager(MessageReceiver receiver, SerializationType serializationType)
        {
            _receiver = receiver;

            _sendQueue = new ConcurrentQueue<MessageBase>();
            _sendReceiveTokenSource = new CancellationTokenSource();
            _receiveStream = new MemoryStream();
            _serializationType = serializationType;
            _isManaging = false;
        }


        #endregion

        #region Public


        /// <summary>
        /// Occurs when [message send receive failed].
        /// </summary>
        public event MessageManagerManagingStoppedEventHandler ManagingStopped;

        /// <summary>
        /// Enqueues the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public bool EnqueueMessage(MessageBase message)
        {
            bool success;
            lock(_managingLock)
            {
                success = _isManaging;
                if(_isManaging)
                {
                    Logger.Instance.LogMessage("Enqueuing message: " + message.Identity, LogPriority.MEDIUM, LogCategory.VERBOSE);
                    _sendQueue.Enqueue(message);
                }
            }

            return success;
        }

        /// <summary>
        /// Starts the managing.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void StartManaging(IConnection connection)
        {
            lock(_managingLock)
            {
                if (_isManaging) { throw new InvalidOperationException("Manager is already managing!"); }

                _isManaging = true;

                _connection = connection;

                _connection.InitializeNewConnection();

                _sendTask = SendAsync();
                _receiveTask = ReceiveAsync();
            }
        }

        /// <summary>
        /// Stops the managing.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void StopManaging()
        {
            StopManaging(null, null);
        }

        /// <summary>
        /// Stops the managing for the given reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public void StopManaging(string reason)
        {
            StopManaging(reason, null);
        }

        #endregion

        #region Private

        /// <summary>
        /// Stops the managing.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="InvalidOperationException">Manager is not currently managing!</exception>
        private void StopManaging(string message, Exception e)
        {
            lock(_managingLock)
            {
                if (_isManaging)
                {
                    Logger.Instance.LogMessage("Stopping managing: [{0}]".FormatStr(message), LogPriority.HIGH, LogCategory.INFO);
                    _isManaging = false;

                    // cancel send and receive
                    Logger.Instance.LogMessage("Cancelling token!", LogPriority.LOW, LogCategory.DEBUG);
                    _sendReceiveTokenSource.Cancel();

                    Logger.Instance.LogMessage("Disconnecting connection in manager!", LogPriority.LOW, LogCategory.DEBUG);
                    // disconnect the connection
                    _connection.DisconnectIfConnected();

                    // dispose of the connection
                    _connection.Dispose();

                    // can't clear -- just create a new one and the old will be GCed
                    _sendQueue = new ConcurrentQueue<MessageBase>();

                    Logger.Instance.LogMessage("Managing stopped!", LogPriority.HIGH, LogCategory.INFO);
                    InvokeManagingStopped(message, e);
                }
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <returns></returns>
        private Task SendAsync()
        {
            Action sendAction = new Action(() =>
            {
                bool stillConnected;
                while (!_sendReceiveTokenSource.IsCancellationRequested)
                {
                    MessageBase toSend;

                    if (_sendQueue.TryDequeue(out toSend))
                    {
                        try
                        {
                            Logger.Instance.LogMessage("Sending [" + toSend.Identity + "] saying [" + ((StringMessage)toSend).Message + "]", LogPriority.LOW, LogCategory.VERBOSE);
                            toSend.TimeSent = DateTime.Now;
                            
                            stillConnected = _connection.Send(toSend, _serializationType);

                            if (!stillConnected)
                            {
                                StopManaging("Connection disconnected", null);
                            }
                        }
                        catch (Exception e)
                        {
                            StopManaging("Exception encountered during sending!", e);
                            break;
                        }
                    }
                }
            });

            //start task
            Task sendTask = new Task(sendAction, _sendReceiveTokenSource.Token, TaskCreationOptions.LongRunning);

            sendTask.Start();

            return sendTask;
        }

        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <returns></returns>
        private Task ReceiveAsync()
        {
            Action receiveAction = new Action(() =>
            {
                MessageBase received = null;
                bool stillConnected;
                while (!_sendReceiveTokenSource.IsCancellationRequested)
                {
                    //_receiveStream.Position = 0;
                    received = null;
                    
                    try
                    {
                        //try receive a message
                        received = _connection.Receive(out stillConnected, _serializationType);

                        if(!stillConnected)
                        {
                            received = null;
                            StopManaging("Connection disconnected", null);
                        }
                    }
                    catch (Exception e)
                    {
                        StopManaging("Exception encountered during receiving!", e);
                        break;
                    }

                    //if there was data received
                    if (received != null)
                    {
                        received.TimeReceived = DateTime.Now;

                        // send the message over to the receiver
                        _receiver.AcceptReceivedMessage(received);
                    }
                }
            });

            Task receiveTask = new Task(receiveAction, _sendReceiveTokenSource.Token, TaskCreationOptions.LongRunning);

            receiveTask.Start();

            return receiveTask;
        }

        /// <summary>
        /// Invokes the send receive failed.
        /// </summary>
        private void InvokeManagingStopped(string message = null, Exception e = null)
        {
            ManagingStopped?.Invoke(new MessageManagerManagingStoppedEventArgs(message, e));
        }
        
        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _receiveStream.Dispose();
                    _connection.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MessageManagerBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
