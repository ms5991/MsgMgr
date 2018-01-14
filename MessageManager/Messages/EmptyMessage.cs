using MsgMgr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Messages
{
    [Serializable]
    public class EmptyMessage : MessageBase
    {
        public EmptyMessage() : base()
        {

        }
    }
}
