using MsgMgr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Net;
using MsgMgrCommon.Logging;

namespace MsgMgr.Connections
{
    public sealed class TcpServer : TcpConnection
    {        
        public TcpServer(string ipAddress, int portNumber) : base(ipAddress, portNumber)
        {
        }

        /// <summary>
        /// Initializes a new connection.  If a connection already exists that conflicts with this connection attempt, the IConnection
        /// implementation should disconnect the existing connection
        /// </summary>
        public override void InitializeNewConnection()
        {
            DisconnectIfConnected();

            TcpListener listener = new TcpListener(IpEndPoint);

            listener.Start();

            Client = listener.AcceptTcpClient().Client;

            Logger.Instance.LogMessage("Connected to " + Client.RemoteEndPoint, LogPriority.MEDIUM, LogCategory.INFO, LogCategory.NETWORK);

            listener.Stop();
        }
    }
}
