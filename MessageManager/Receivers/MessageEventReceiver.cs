using MsgMgr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Receivers
{
    public class EventBasedMessageReceiver : MessageReceiver
    {
        public EventBasedMessageReceiver(bool invokeMessageInvokeOnReceive) : base(invokeMessageInvokeOnReceive)
        {
        }

        /// <summary>
        /// Occurs when [message received].
        /// </summary>
        public event MessageManagerMessageReceivedEventHandler MessageReceived;
        
        /// <summary>
        /// Accepts a received message by raising [MessageReceived] event.
        /// </summary>
        /// <param name="message">The message.</param>
        internal override void AcceptReceivedMessage(MessageBase message)
        {
            base.AcceptReceivedMessage(message);
            MessageReceived?.Invoke(new MessageManagerMessageReceivedEventArgs(message));
        }
        
    }
}
