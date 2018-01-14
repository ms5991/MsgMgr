using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Utilities
{
    public class ReceiveException : Exception
    {
        public ReceiveException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
