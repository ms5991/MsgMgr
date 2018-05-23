using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsgMgrCommon.Extensions
{
    public static class ICollectionExtensions
    {
        public static bool ContainsAny<T>(this ICollection<T> collection, params T[] items)
        {
            for(int i=0;i< items.Length;i++)
            {
                if(collection.Contains(items[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddRange<T>(this ICollection<T> collection , params T[] items)
        {
            for(int i=0;i<items.Length;i++)
            {
                collection.Add(items[i]);
            }
        }
    }
}
