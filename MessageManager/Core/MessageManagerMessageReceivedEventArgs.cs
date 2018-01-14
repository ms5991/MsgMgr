using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Core
{
    public delegate void MessageManagerMessageReceivedEventHandler(MessageManagerMessageReceivedEventArgs e);

    public class MessageManagerMessageReceivedEventArgs : EventArgs
    {
        public MessageBase Message { get; private set; }

        public MessageManagerMessageReceivedEventArgs(MessageBase _message)
        {
            Message = _message;
        }
    }
}
