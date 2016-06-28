using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTTPRequestLib
{
    internal static class Extentions
    {
        public static bool TryGetValue(this WebHeaderCollection headers, string key, out string val)
        {
            bool success = false;
            val = "";

            if (headers.AllKeys.Contains(key))
            {
                val = headers[key];
                success = true;
            }

            return success;
        }
    }
}
