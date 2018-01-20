using System;

namespace MsgMgr.Core
{
    public interface IConnection : IDisposable
    {
        /// <summary>
        /// Initializes a new connection.  If a connection already exists that conflicts with this connection attempt, the IConnection 
        /// implementation should disconnect the existing connection.
        /// </summary>
        void InitializeNewConnection();
        
        /// <summary>
        /// Disconnects the IConnection if connected.
        /// </summary>
        void DisconnectIfConnected();

        /// <summary>
        /// Performs a single send action, sending the specified message.  Return value indicates whether the IConnection is still connected
        /// </summary>
        /// <param name="toSend">Message to send.</param>
        /// <returns></returns>
        bool Send(MessageBase toSend);

        /// <summary>
        /// Performs a single receive attempt.  Returns the resulting data from the receive, and the out parameter indicates whether the IConnection is still connected.
        /// </summary>
        /// <param name="stillConnected">if set to <c>true</c> [still connected].</param>
        /// <returns>The received data</returns>
        MessageBase Receive(out bool stillConnected);
    }
}
