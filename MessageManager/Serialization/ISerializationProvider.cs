using MsgMgr.Core;
using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    internal interface ISerializationProvider
    {
        void Begin();
        
        void AppendObject(Type objectType, string propertyName, object value);

        string End();
    }
}
