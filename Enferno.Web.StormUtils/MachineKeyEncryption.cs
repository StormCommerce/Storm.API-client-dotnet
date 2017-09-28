
using System;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Enferno.Web.StormUtils
{
    public static class MachineKeyEncryption
    {
        public static string Encode(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var buf = Encoding.UTF8.GetBytes(text);
            buf = MachineKey.Protect(buf);
            return HttpServerUtility.UrlTokenEncode(buf);
        }

        public static string Decode(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            try
            {
                var buf = HttpServerUtility.UrlTokenDecode(text);
                buf = MachineKey.Unprotect(buf);
                return buf != null ? Encoding.UTF8.GetString(buf, 0, buf.Length) : null;
            }
            catch (Exception)
            {
                return TryOldDecode(text);
            }
        }

        private static string TryOldDecode(string text)
        {
            // Detta är bara med som support för gamla cookies under en kort tid. Tas med fördel bort i release efter .net 4.5.
            var buf = MachineKey.Decode(text, MachineKeyProtection.All);
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }
    }
}
