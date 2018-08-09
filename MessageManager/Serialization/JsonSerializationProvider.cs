using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgr.Serialization
{
    internal class JsonSerializationProvider : SerializationProviderBase
    {
        public override SerializationType SerializationType { get { return SerializationType.JSON; } }

        internal JsonSerializationProvider(string name) : base(name)
        {
        }

        public override void Begin()
        {
            throw new NotImplementedException();
        }

        public override void AppendObject(Type objectType, string propertyName, object value)
        {
            throw new NotImplementedException();
        }

        public override string End()
        {
            throw new NotImplementedException();
        }
    }
}
