using MsgMgr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Receivers
{
    public class EventBasedMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// Occurs when [message received].
        /// </summary>
        public event MessageManagerMessageReceivedEventHandler MessageReceived;
        
        /// <summary>
        /// Accepts a received message by raising [MessageReceived] event.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AcceptReceivedMessage(MessageBase message)
        {
            MessageReceived?.Invoke(new MessageManagerMessageReceivedEventArgs(message));
        }
        
    }
}
