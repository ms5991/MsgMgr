using MsgMgr.Core;
using MsgMgrCommon.Extensions;
using MsgMgrCommon.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Messages
{
    [Serializable]
    public class StringMessage : MessageBase
    {
        public string Message { get; }

        public StringMessage(string message) : base()
        {
            Message = message;
        }

        public override void InvokeOnReceieve()
        {
            Logger.Instance.LogMessage("Hello, I'm [{0}] and I say: [{1}]".FormatStr(this.Identity, this.Message), LogPriority.HIGH, LogCategory.INFO);
        }
    }
}
