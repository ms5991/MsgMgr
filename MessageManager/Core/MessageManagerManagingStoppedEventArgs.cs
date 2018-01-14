using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Core
{
    public delegate void MessageManagerManagingStoppedEventHandler(MessageManagerManagingStoppedEventArgs e);

    public class MessageManagerManagingStoppedEventArgs : EventArgs
    {
        public string Message { get; }

        public Exception Exception { get; }

        public MessageManagerManagingStoppedEventArgs(string message, Exception exception = null)
        {
            Message = message;
            Exception = exception;
        }
    }
}
