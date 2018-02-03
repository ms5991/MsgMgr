using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Core
{
    public interface IMessageReceiver
    {
        /// <summary>
        /// Accepts a received message.
        /// </summary>
        /// <param name="message">The message.</param>
        void AcceptReceivedMessage(MessageBase message);
    }
}
