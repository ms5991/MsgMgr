using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgrCommon.Extensions
{
    public static class StringExtensions
    {
        public static string FormatStr(this string str, params object[] obj)
        {
            return string.Format(str, obj);
        }
    }
}
