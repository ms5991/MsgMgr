using MsgMgrCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    internal abstract class DeserializationProviderBase : IDeserializationProvider
    {
        public static IDeserializationProvider GetProvider(SerializationType type)
        {
            DeserializationProviderBase provider;

            switch (type)
            {
                case SerializationType.XML:
                    provider = new XmlDeserializationProvider();
                    break;
                case SerializationType.JSON:
                default:
                    throw new InvalidOperationException("Unsupported serialization type: [{0}]".FormatStr(type));
            }

            return provider;
        }

        public abstract SerializableBase Deserialize(byte[] asciiRep);
    }
}
