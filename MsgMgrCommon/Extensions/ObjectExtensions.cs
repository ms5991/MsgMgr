using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgrCommon.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsNotNull(this object obj)
        {
            return !obj.IsNull();
        }

        public static void ThrowIfNull(this object obj, string message)
        {
            if(obj.IsNull())
            {
                throw new InvalidOperationException("[ThrowIfNull]: " + message);
            }
        }

        public static void ThrowIfNotNull(this object obj, string message)
        {
            if (obj.IsNotNull())
            {
                throw new InvalidOperationException("[ThrowIfNotNull]: " + message);
            }
        }
    }
}
