using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgrCommon.Extensions
{
    public static class IntExtensions
    {
        public static byte[] Serialize(this int theInt)
        {
            return BitConverter.GetBytes(theInt);
        }
    }
}
