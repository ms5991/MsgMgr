using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Core
{
    public abstract class MessageReceiver
    {
        private bool _invokeMessageInvokeOnReceive;

        /// <summary>
        /// If true, the receiver will call InvokeOnReceive() on every message it receives
        /// </summary>
        /// <value>
        ///   <c>true</c> if [invoke message invoke on receive]; otherwise, <c>false</c>.
        /// </value>
        public bool InvokeMessageInvokeOnReceive
        {
            get
            {
                return _invokeMessageInvokeOnReceive;
            }
            set
            {
                _invokeMessageInvokeOnReceive = value;
            }
        }


        protected MessageReceiver(bool invokeMessageInvokeOnReceive)
        {
            _invokeMessageInvokeOnReceive = invokeMessageInvokeOnReceive;
        }


        /// <summary>
        /// Accepts a received message.
        /// </summary>
        /// <param name="message">The message.</param>
        internal virtual void AcceptReceivedMessage(MessageBase message)
        {
            if(InvokeMessageInvokeOnReceive)
            {
                message.InvokeOnReceieve();
            }
        }
    }
}
