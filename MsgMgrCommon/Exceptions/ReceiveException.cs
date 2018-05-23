using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgrCommon.Exceptions
{ 
    public class ReceiveException : Exception
    {
        public ReceiveException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
