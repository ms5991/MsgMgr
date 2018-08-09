using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    internal interface IDeserializationProvider
    {
        SerializableBase Deserialize(byte[] asciiRep);
    }
}
