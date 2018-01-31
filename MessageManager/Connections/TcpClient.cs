using MsgMgr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MsgMgr.Utilities;

namespace MsgMgr.Connections
{
    public sealed class TcpClient : TcpConnection
    {
        public TcpClient(string ipAddress, int portNumber) : base(ipAddress, portNumber)
        {
        }

        /// <summary>
        /// Initializes a new connection.  If a connection already exists that conflicts with this connection attempt, the IConnection
        /// implementation should disconnect the existing connection
        /// </summary>
        public override void InitializeNewConnection()
        {
            DisconnectIfConnected();

            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool connected = false;

            //while unable to connect, keep trying
            while (!connected)
            {
                try
                {
                    Client.Connect(IpEndPoint);
                    connected = true;
                }
                catch (Exception)
                {
                    connected = false;
                }
            }

            Logger.Instance.LogMessage("Connected to " + Client.RemoteEndPoint, LogPriority.MEDIUM, LogCategory.INFO, LogCategory.NETWORK);
        }
    }
}
