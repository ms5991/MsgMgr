using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Performs a single send action, sending the specified data, which is of the specified length.  Return value indicates whether the IConnection is still connected
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length.</param>
        /// <returns>Whether this instance is still connected</returns>
        bool Send(byte[] data, int length);

        /// <summary>
        /// Performs a single receive attempt.  Returns the resulting data from the receive, and the out parameter indicates whether the IConnection is still connected
        /// </summary>
        /// <param name="stillConnected">if set to <c>true</c> [still connected].</param>
        /// <returns>The received data</returns>
        byte[] Receive(out bool stillConnected);
    }
}
